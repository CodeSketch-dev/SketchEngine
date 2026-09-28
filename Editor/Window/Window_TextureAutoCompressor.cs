#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace SketchEngine.Editor
{
    public class Window_TextureAutoCompressor : EditorWindow
    {
        readonly List<TextureEntry> _textureEntries = new List<TextureEntry>();

        DefaultAsset _folderAsset;
        bool _applyAndroidOverride = true;
        bool _applyIOSOverride = true;

        readonly string[] _compressionOptions = { "None", "Low", "Normal", "High" };
        readonly string[] _maxSizeOptions = { "Auto", "32", "64", "128", "256", "512", "1024", "2048" };

        ReorderableList _reorderableList;

        bool _globalGenerateMipMaps = false;
        bool _globalAlphaIsTransparency = true;
        bool _useLowForAutoSize = false;

        Vector2 _scrollPos;

        // =====================================================
        // MENU
        // =====================================================

        [MenuItem("CodeSketch/Tools/Texture/Texture Auto Compressor")]
        public static void ShowWindow()
        {
            GetWindow<Window_TextureAutoCompressor>("Texture Auto Compressor");
        }

        void OnEnable()
        {
            CreateList();
        }

        void CreateList()
        {
            _reorderableList = new ReorderableList(
                _textureEntries,
                typeof(TextureEntry),
                true, true, false, false
            );

            _reorderableList.drawElementCallback = DrawTextureEntry;
            _reorderableList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Danh sách Texture", EditorStyles.boldLabel);
            };
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            GUILayout.Label("Chọn folder chứa image", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            var newFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Folder",
                _folderAsset,
                typeof(DefaultAsset),
                false
            );

            if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), GUILayout.Width(30)))
            {
                _folderAsset = newFolder;
                LoadTextures();
            }
            EditorGUILayout.EndHorizontal();

            if (newFolder != _folderAsset)
            {
                _folderAsset = newFolder;
                LoadTextures();
            }

            GUILayout.Label("Hoặc:", EditorStyles.boldLabel);
            DrawDragDropTexturesArea();

            _applyAndroidOverride = EditorGUILayout.Toggle("Override for Android", _applyAndroidOverride);
            _applyIOSOverride = EditorGUILayout.Toggle("Override for iOS", _applyIOSOverride);

            GUILayout.Space(10);
            GUILayout.Label("Tùy chọn áp dụng cho tất cả:", EditorStyles.boldLabel);
            _globalGenerateMipMaps = EditorGUILayout.Toggle("Generate MipMaps", _globalGenerateMipMaps);
            _globalAlphaIsTransparency =
                EditorGUILayout.Toggle("Alpha Is Transparency", _globalAlphaIsTransparency);

            GUILayout.Space(10);
            GUILayout.Label("Chế độ Auto Size:", EditorStyles.boldLabel);
            _useLowForAutoSize =
                EditorGUILayout.Toggle("Use Low Side for Auto", _useLowForAutoSize);

            GUILayout.Space(5);

            _scrollPos = EditorGUILayout.BeginScrollView(
                _scrollPos,
                GUILayout.Height(position.height - 350)
            );

            if (_textureEntries.Count > 0 && _reorderableList != null)
            {
                _reorderableList.DoLayoutList();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Không tìm thấy texture nào trong thư mục.",
                    MessageType.Info
                );
            }

            EditorGUILayout.EndScrollView();

            GUILayout.FlexibleSpace();
            GUILayout.Space(10);

            if (GUILayout.Button("Apply Settings", GUILayout.Height(35)))
            {
                ApplySettings();
            }

            EditorGUILayout.EndVertical();
        }

        // =====================================================
        // LOAD
        // =====================================================

        void LoadTextures()
        {
            _textureEntries.Clear();
            if (_folderAsset == null) return;

            string folderPath = AssetDatabase.GetAssetPath(_folderAsset);
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (texture == null) continue;

                _textureEntries.Add(new TextureEntry
                {
                    _texture = texture,
                    _path = assetPath,
                    _compression = TextureImporterCompression.Compressed,
                    _maxSizeIndex = 0
                });
            }

            CreateList();
            Repaint();
        }

        // =====================================================
        // DRAW ITEM
        // =====================================================

        void DrawTextureEntry(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= _textureEntries.Count) return;

            var entry = _textureEntries[index];
            float x = rect.x;
            float y = rect.y + 2;
            float h = EditorGUIUtility.singleLineHeight;

            if (entry._texture == null)
            {
                EditorGUI.LabelField(
                    new Rect(x, y, 300, h),
                    "[Texture đã bị xóa hoặc null]",
                    EditorStyles.miniLabel
                );
                return;
            }

            EditorGUI.LabelField(new Rect(x, y, 120, h), entry._texture.name);

            EditorGUI.BeginChangeCheck();
            int newCompressionIndex = EditorGUI.Popup(
                new Rect(x + 130, y, 80, h),
                GetCompressionIndex(entry._compression),
                _compressionOptions
            );

            if (EditorGUI.EndChangeCheck())
            {
                entry._compression = GetCompressionFromIndex(newCompressionIndex);
            }

            EditorGUI.BeginChangeCheck();
            int newSizeIndex = EditorGUI.Popup(
                new Rect(x + 220, y, 60, h),
                entry._maxSizeIndex,
                _maxSizeOptions
            );

            if (EditorGUI.EndChangeCheck())
            {
                entry._maxSizeIndex = newSizeIndex;
            }

            if (GUI.Button(new Rect(rect.xMax - 25, y, 20, h), "X"))
            {
                _textureEntries.RemoveAt(index);
                CreateList();
                GUIUtility.ExitGUI();
            }
        }

        // =====================================================
        // DRAG & DROP
        // =====================================================

        void DrawDragDropTexturesArea()
        {
            Rect dropArea = GUILayoutUtility.GetRect(0, 60, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Kéo ảnh vào đây", EditorStyles.helpBox);

            Event evt = Event.current;
            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) &&
                dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var draggedObj in DragAndDrop.objectReferences)
                    {
                        if (draggedObj is Texture2D tex)
                        {
                            string path = AssetDatabase.GetAssetPath(tex);
                            if (!string.IsNullOrEmpty(path) &&
                                !_textureEntries.Exists(e => e._path == path))
                            {
                                _textureEntries.Add(new TextureEntry
                                {
                                    _texture = tex,
                                    _path = path,
                                    _compression = TextureImporterCompression.Compressed,
                                    _maxSizeIndex = 0
                                });
                            }
                        }
                    }

                    CreateList();
                    Repaint();
                }

                evt.Use();
            }
        }

        // =====================================================
        // APPLY
        // =====================================================

        void ApplySettings()
        {
            int count = 0;

            foreach (var entry in _textureEntries)
            {
                if (entry._texture == null || string.IsNullOrEmpty(entry._path))
                    continue;

                var importer =
                    AssetImporter.GetAtPath(entry._path) as TextureImporter;
                if (importer == null) continue;

                importer.mipmapEnabled = _globalGenerateMipMaps;
                importer.alphaIsTransparency = _globalAlphaIsTransparency;

                int maxSize;

                if (entry._maxSizeIndex == 0)
                {
                    int dimension = _useLowForAutoSize
                        ? Mathf.Min(entry._texture.width, entry._texture.height)
                        : Mathf.Max(entry._texture.width, entry._texture.height);

                    int nearestSize = GetAdjustedSize(dimension);
                    if (dimension > 2048 || nearestSize > 1024)
                        nearestSize = 1024;

                    maxSize = nearestSize;
                }
                else
                {
                    maxSize = int.Parse(_maxSizeOptions[entry._maxSizeIndex]);
                }

                var defaultSettings = importer.GetDefaultPlatformTextureSettings();
                defaultSettings.overridden = true;
                defaultSettings.textureCompression = entry._compression;
                defaultSettings.maxTextureSize = maxSize;
                defaultSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                defaultSettings.format = GetDefaultFormat(entry._compression);
                importer.SetPlatformTextureSettings(defaultSettings);

                if (_applyAndroidOverride)
                {
                    importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                    {
                        name = "Android",
                        overridden = true,
                        maxTextureSize = maxSize,
                        format = GetASTCFormat(maxSize),
                        resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                        textureCompression = TextureImporterCompression.Compressed
                    });
                }

                if (_applyIOSOverride)
                {
                    importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                    {
                        name = "iPhone",
                        overridden = true,
                        maxTextureSize = maxSize,
                        format = GetASTCFormat(maxSize),
                        resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                        textureCompression = TextureImporterCompression.Compressed
                    });
                }

                importer.SaveAndReimport();
                count++;
            }

            EditorUtility.DisplayDialog(
                "Hoàn tất",
                $"Đã chỉnh {count} texture.",
                "OK"
            );
        }

        // =====================================================
        // HELPERS
        // =====================================================

        int GetCompressionIndex(TextureImporterCompression compression)
        {
            return compression switch
            {
                TextureImporterCompression.Uncompressed => 0,
                TextureImporterCompression.CompressedLQ => 1,
                TextureImporterCompression.Compressed => 2,
                TextureImporterCompression.CompressedHQ => 3,
                _ => 2
            };
        }

        TextureImporterCompression GetCompressionFromIndex(int index)
        {
            return index switch
            {
                0 => TextureImporterCompression.Uncompressed,
                1 => TextureImporterCompression.CompressedLQ,
                2 => TextureImporterCompression.Compressed,
                3 => TextureImporterCompression.CompressedHQ,
                _ => TextureImporterCompression.Compressed
            };
        }

        TextureImporterFormat GetDefaultFormat(TextureImporterCompression compression)
        {
            return compression switch
            {
                TextureImporterCompression.Uncompressed => TextureImporterFormat.RGBA32,
                TextureImporterCompression.CompressedLQ => TextureImporterFormat.RGB16,
                TextureImporterCompression.Compressed => TextureImporterFormat.RGBA32,
                TextureImporterCompression.CompressedHQ => TextureImporterFormat.RGBAFloat,
                _ => TextureImporterFormat.RGBA32
            };
        }

        TextureImporterFormat GetASTCFormat(int maxSize)
        {
            if (maxSize <= 128) return TextureImporterFormat.ASTC_4x4;
            if (maxSize <= 512) return TextureImporterFormat.ASTC_6x6;
            return TextureImporterFormat.ASTC_8x8;
        }

        int GetAdjustedSize(int size)
        {
            int[] options = { 32, 64, 128, 256, 512, 1024, 2048 };
            foreach (int opt in options)
            {
                if (size <= opt)
                {
                    if (opt > size + 200)
                    {
                        int index = System.Array.IndexOf(options, opt);
                        return options[Mathf.Max(0, index - 1)];
                    }
                    return opt;
                }
            }
            return 2048;
        }

        // =====================================================
        // DATA
        // =====================================================

        class TextureEntry
        {
            public Texture2D _texture;
            public string _path;
            public TextureImporterCompression _compression;
            public int _maxSizeIndex;
        }
    }
}
#endif
