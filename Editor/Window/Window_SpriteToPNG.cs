using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SketchEngine.Editor
{
    public class Window_SpriteToPNG : SketchToolTab
    {
        public override string Title => "Sprite To PNG";

        const string DefaultSavePath = "Assets/_SpriteToPNG";

        readonly List<Object> _items = new List<Object>(); // Sprite hoặc Texture2D
        string _savePath = DefaultSavePath;
        Vector2 _scroll;
        string _status = "";

        public override void OnGUI()
        {
            GUILayout.Space(6);

            DrawSection("1. Nguồn ảnh", () =>
            {
                DrawDropArea();
                GUILayout.Space(4);
                DrawList();
            });

            DrawSection("2. Nơi lưu", () =>
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(_savePath, EditorStyles.miniLabel);
                if (GUILayout.Button("Chọn thư mục...", GUILayout.Width(120)))
                {
                    string selected = EditorUtility.SaveFolderPanel("Chọn nơi lưu", _savePath, "");
                    if (!string.IsNullOrEmpty(selected))
                        _savePath = FileUtil.GetProjectRelativePath(selected);
                }
                EditorGUILayout.EndHorizontal();
            });

            DrawSection("3. Xuất", () =>
            {
                EditorGUILayout.BeginHorizontal();

                using (new EditorGUI.DisabledScope(_items.Count == 0))
                {
                    if (GUILayout.Button($"Convert & Save PNG ({_items.Count})", GUILayout.Height(28)))
                        ConvertAll();
                }

                using (new EditorGUI.DisabledScope(_items.Count == 0))
                {
                    if (GUILayout.Button("Xoá hết", GUILayout.Width(90), GUILayout.Height(28)))
                    {
                        _items.Clear();
                        _status = "";
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(_status))
                {
                    GUILayout.Space(4);
                    EditorGUILayout.HelpBox(_status, MessageType.Info);
                }
            });
        }

        // Khối có tiêu đề, bọc trong helpBox để các phần tách bạch.
        static void DrawSection(string title, System.Action content)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(title, EditorStyles.boldLabel);
            GUILayout.Space(2);
            content();
            EditorGUILayout.EndVertical();
            GUILayout.Space(6);
        }

        // =====================================================
        // DRAG & DROP
        // =====================================================

        void DrawDropArea()
        {
            Rect area = GUILayoutUtility.GetRect(0, 64, GUILayout.ExpandWidth(true));
            GUI.Box(area, "Kéo Sprite / Texture2D vào đây", EditorStyles.helpBox);

            Event evt = Event.current;
            if (!area.Contains(evt.mousePosition))
                return;

            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    AddDropped(DragAndDrop.objectReferences);
                }

                evt.Use();
            }
        }

        void AddDropped(Object[] objects)
        {
            foreach (Object obj in objects)
            {
                if ((obj is Sprite || obj is Texture2D) && !_items.Contains(obj))
                    _items.Add(obj);
            }
        }

        void DrawList()
        {
            if (_items.Count == 0)
            {
                GUILayout.Label("Chưa có ảnh nào được chọn.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.Height(240));

            for (int i = 0; i < _items.Count; i++)
            {
                Object item = _items[i];

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                Texture thumb = AssetPreview.GetMiniThumbnail(item);
                GUILayout.Label(thumb, GUILayout.Width(40), GUILayout.Height(40));

                EditorGUILayout.BeginVertical();
                GUILayout.Label(item.name, EditorStyles.boldLabel);
                GUILayout.Label(item is Sprite ? "Sprite" : "Texture2D", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Xoá", GUILayout.Width(50)))
                {
                    _items.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // =====================================================
        // CONVERT
        // =====================================================

        void ConvertAll()
        {
            Directory.CreateDirectory(_savePath);

            var usedNames = new HashSet<string>();
            int saved = 0;

            foreach (Object item in _items)
            {
                if (!TryGetImage(item, out Texture2D source, out Rect rect))
                    continue;

                string fileName = UniqueFileName(GetBaseName(item), usedNames);
                Texture2D readable = ReadRect(source, rect);
                File.WriteAllBytes(Path.Combine(_savePath, fileName + ".png"), readable.EncodeToPNG());
                Object.DestroyImmediate(readable);
                saved++;
            }

            AssetDatabase.Refresh();
            _status = $"Đã lưu {saved}/{_items.Count} ảnh vào {_savePath}";
            Debug.Log($"[SpriteToPNG] {_status}");
        }

        static bool TryGetImage(Object item, out Texture2D texture, out Rect rect)
        {
            texture = null;
            rect = default;

            if (item is Sprite sprite && sprite.texture != null)
            {
                texture = sprite.texture;
                rect = sprite.rect;
                return true;
            }

            if (item is Texture2D tex)
            {
                texture = tex;
                rect = new Rect(0, 0, tex.width, tex.height);
                return true;
            }

            return false;
        }

        static string GetBaseName(Object item)
        {
            return string.IsNullOrEmpty(item.name) ? "image" : item.name;
        }

        static string UniqueFileName(string name, HashSet<string> used)
        {
            string candidate = name;
            int index = 1;
            while (!used.Add(candidate))
                candidate = $"{name}_{index++}";
            return candidate;
        }

        // Đọc vùng rect của texture (kể cả texture không bật Read/Write) qua RenderTexture.
        static Texture2D ReadRect(Texture2D source, Rect rect)
        {
            int width = Mathf.Max(1, (int)rect.width);
            int height = Mathf.Max(1, (int)rect.height);

            RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture prev = RenderTexture.active;

            Graphics.Blit(source, rt);
            RenderTexture.active = rt;

            var readable = new Texture2D(width, height, TextureFormat.RGBA32, false);
            // Unity gốc Y ở dưới, còn ReadPixels đọc từ Y dưới: đổi hệ tọa độ của vùng rect.
            readable.ReadPixels(new Rect(rect.x, source.height - rect.y - rect.height, width, height), 0, 0);
            readable.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            return readable;
        }
    }
}
