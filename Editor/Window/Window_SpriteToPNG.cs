using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SketchEngine.Editor
{
    public class Window_SpriteToPNG : EditorWindow
    {
        readonly List<Sprite> spriteList = new(); // Danh sách Sprite
        string savePath = "Assets/_SpriteToPNG"; // Đường dẫn lưu mặc định

        [MenuItem("CodeSketch/Tools/Window/Sprite To PNG")]
        public static void ShowWindow()
        {
            GetWindow<Window_SpriteToPNG>("Sprite To PNG");
        }

        void OnGUI()
        {
            GUILayout.Label("Drag & Drop Sprites Here", EditorStyles.boldLabel);

            // Kéo thả nhiều Sprite vào danh sách
            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object obj in DragAndDrop.objectReferences)
                        if (obj is Sprite sprite && !spriteList.Contains(sprite))
                            spriteList.Add(sprite);
                }

                Event.current.Use();
            }

            // Hiển thị danh sách Sprite
            if (spriteList.Count > 0)
            {
                GUILayout.Label("Selected Sprites:", EditorStyles.boldLabel);
                for (var i = 0; i < spriteList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(spriteList[i], typeof(Sprite), false);
                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        spriteList.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("No sprites selected.", EditorStyles.miniLabel);
            }

            GUILayout.Space(10);

            // Chọn nơi lưu file
            EditorGUILayout.LabelField("Save Path:", savePath);
            if (GUILayout.Button("Choose Save Folder"))
            {
                var selectedPath = EditorUtility.SaveFolderPanel("Choose Save Location", savePath, "");
                if (!string.IsNullOrEmpty(selectedPath)) savePath = FileUtil.GetProjectRelativePath(selectedPath);
            }

            GUILayout.Space(10);

            // Nút Convert
            GUI.enabled = spriteList.Count > 0;
            if (GUILayout.Button("Convert & Save PNGs")) ConvertSpritesToPNG();
            GUI.enabled = true;
        }

        void ConvertSpritesToPNG()
        {
            foreach (Sprite sprite in spriteList)
            {
                var filePath = Path.Combine(savePath, sprite.name + ".png");
                SaveTextureAsPNG(SpriteToTexture2D(sprite), filePath);
            }

            AssetDatabase.Refresh(); // Làm mới Unity Editor
            Debug.Log($"Saved {spriteList.Count} sprites to {savePath}");
        }

        private Texture2D SpriteToTexture2D(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
            {
                Debug.LogError("Sprite or Texture is null!");
                return null;
            }

            // Lấy vùng Rect của Sprite
            Rect rect = sprite.rect;
            Texture2D sourceTex = sprite.texture;

            // Tạo bản sao Texture2D có thể đọc
            Texture2D readableTex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);

            // Copy pixels từ sprite texture gốc
            RenderTexture rt = RenderTexture.GetTemporary(sourceTex.width, sourceTex.height);
            Graphics.Blit(sourceTex, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
    
            readableTex.ReadPixels(new Rect(rect.x, sourceTex.height - rect.y - rect.height, rect.width, rect.height), 0, 0);
            readableTex.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            return readableTex;
        }


        void SaveTextureAsPNG(Texture2D texture, string filePath)
        {
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"Saved PNG: {filePath}");
        }
    }
}