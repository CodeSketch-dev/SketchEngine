#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SketchEngine.Editor
{
    public class Window_TextureAutoCompressor : SketchToolTab
    {
        // Mức nén: càng về cuối block ASTC càng lớn -> nén càng mạnh, chất lượng càng thấp.
        public enum CompressionLevel
        {
            Astc4x4,
            Astc6x6,
            Astc8x8,
            Astc10x10,
            Astc12x12,
            Uncompressed
        }

        static readonly string[] CompressionLabels =
        {
            "ASTC 4x4 - nén ít nhất (chất lượng cao)",
            "ASTC 6x6 - cân bằng",
            "ASTC 8x8 - nén nhiều",
            "ASTC 10x10 - nén mạnh",
            "ASTC 12x12 - nén mạnh nhất",
            "Không nén (RGBA32)"
        };

        static readonly string[] MaxSizeOptions = { "Auto", "32", "64", "128", "256", "512", "1024", "2048" };

        public override string Title => "Texture Auto Compressor";

        const CompressionLevel DefaultLevel = CompressionLevel.Astc4x4;

        readonly List<TextureEntry> _textureEntries = new List<TextureEntry>();
        ReorderableList _reorderableList;

        DefaultAsset _folderAsset;
        bool _applyAndroidOverride = true;
        bool _applyIOSOverride = true;

        bool _globalGenerateMipMaps;
        bool _globalAlphaIsTransparency = true;
        bool _useLowForAutoSize;

        Vector2 _scrollPos;

        public override void OnEnable()
        {
            CreateList();
        }

        void CreateList()
        {
            _reorderableList = new ReorderableList(_textureEntries, typeof(TextureEntry), true, true, false, false)
            {
                drawElementCallback = DrawTextureEntry,
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Danh sách Texture", EditorStyles.boldLabel),
                elementHeight = EditorGUIUtility.singleLineHeight + 6
            };
        }

        public override void OnGUI()
        {
            DrawFolderSection();
            DrawGlobalSection();
            DrawListSection();
            DrawApplySection();
        }

        // =====================================================
        // SECTIONS
        // =====================================================

        void DrawFolderSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("1. Nguồn ảnh", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            var newFolder = (DefaultAsset)EditorGUILayout.ObjectField("Folder", _folderAsset, typeof(DefaultAsset), false);

            if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), GUILayout.Width(30)))
                LoadTextures();
            EditorGUILayout.EndHorizontal();

            if (newFolder != _folderAsset)
            {
                _folderAsset = newFolder;
                LoadTextures();
            }

            DrawDragDropTexturesArea();
            EditorGUILayout.EndVertical();
            GUILayout.Space(6);
        }

        void DrawGlobalSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("2. Tùy chọn chung", EditorStyles.boldLabel);

            _globalGenerateMipMaps = EditorGUILayout.Toggle("Generate MipMaps", _globalGenerateMipMaps);
            _globalAlphaIsTransparency = EditorGUILayout.Toggle("Alpha Is Transparency", _globalAlphaIsTransparency);
            _useLowForAutoSize = EditorGUILayout.Toggle("Auto Size theo cạnh nhỏ", _useLowForAutoSize);

            GUILayout.Space(4);
            GUILayout.Label("Platform mobile (ASTC)", EditorStyles.miniBoldLabel);
            _applyAndroidOverride = EditorGUILayout.Toggle("Android", _applyAndroidOverride);
            _applyIOSOverride = EditorGUILayout.Toggle("iOS", _applyIOSOverride);

            EditorGUILayout.EndVertical();
            GUILayout.Space(6);
        }

        void DrawListSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"3. Danh sách ({_textureEntries.Count} texture)", EditorStyles.boldLabel);

            if (_textureEntries.Count == 0)
            {
                EditorGUILayout.HelpBox("Chọn folder hoặc kéo ảnh vào để bắt đầu.", MessageType.Info);
            }
            else
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.MinHeight(160), GUILayout.MaxHeight(420));
                _reorderableList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        void DrawApplySection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUI.DisabledScope(_textureEntries.Count == 0))
            {
                if (GUILayout.Button($"Apply Settings ({_textureEntries.Count})", GUILayout.Height(32)))
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

            if (_folderAsset != null)
            {
                string folderPath = AssetDatabase.GetAssetPath(_folderAsset);
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    if (texture != null)
                        AddEntry(texture, assetPath);
                }
            }

            CreateList();
            Repaint();
        }

        void AddEntry(Texture2D texture, string path)
        {
            _textureEntries.Add(new TextureEntry
            {
                _texture = texture,
                _path = path,
                _level = DefaultLevel,
                _maxSizeIndex = 0
            });
        }

        // =====================================================
        // ITEM
        // =====================================================

        void DrawTextureEntry(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= _textureEntries.Count) return;

            var entry = _textureEntries[index];
            rect.y += 3;
            float h = EditorGUIUtility.singleLineHeight;

            if (entry._texture == null)
            {
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, h), "[Texture đã bị xóa]", EditorStyles.miniLabel);
                return;
            }

            float nameWidth = rect.width * 0.25f;
            float sizeWidth = 110f;
            float removeWidth = 24f;
            float levelWidth = rect.width - nameWidth - sizeWidth - removeWidth - 12f;

            EditorGUI.LabelField(new Rect(rect.x, rect.y, nameWidth, h), entry._texture.name);

            EditorGUI.BeginChangeCheck();
            int levelIndex = EditorGUI.Popup(
                new Rect(rect.x + nameWidth + 4, rect.y, levelWidth, h),
                (int)entry._level,
                CompressionLabels);
            if (EditorGUI.EndChangeCheck())
                entry._level = (CompressionLevel)levelIndex;

            EditorGUI.BeginChangeCheck();
            int sizeIndex = EditorGUI.Popup(
                new Rect(rect.xMax - sizeWidth - removeWidth - 8, rect.y, sizeWidth, h),
                entry._maxSizeIndex,
                MaxSizeOptions);
            if (EditorGUI.EndChangeCheck())
                entry._maxSizeIndex = sizeIndex;

            if (GUI.Button(new Rect(rect.xMax - removeWidth, rect.y, removeWidth, h), "X"))
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
            Rect dropArea = GUILayoutUtility.GetRect(0, 48, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Kéo Texture2D vào đây", EditorStyles.helpBox);

            Event evt = Event.current;
            if ((evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) || !dropArea.Contains(evt.mousePosition))
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (var dragged in DragAndDrop.objectReferences)
                {
                    if (dragged is not Texture2D tex) continue;

                    string path = AssetDatabase.GetAssetPath(tex);
                    if (!string.IsNullOrEmpty(path) && !_textureEntries.Exists(e => e._path == path))
                        AddEntry(tex, path);
                }

                CreateList();
                Repaint();
            }

            evt.Use();
        }

        // =====================================================
        // APPLY
        // =====================================================

        void ApplySettings()
        {
            int count = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var entry in _textureEntries)
                {
                    if (entry._texture == null || string.IsNullOrEmpty(entry._path))
                        continue;

                    var importer = AssetImporter.GetAtPath(entry._path) as TextureImporter;
                    if (importer == null) continue;

                    importer.mipmapEnabled = _globalGenerateMipMaps;
                    importer.alphaIsTransparency = _globalAlphaIsTransparency;

                    int maxSize = ResolveMaxSize(importer, entry);

                    var defaultSettings = importer.GetDefaultPlatformTextureSettings();
                    defaultSettings.overridden = true;
                    defaultSettings.maxTextureSize = maxSize;
                    defaultSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                    defaultSettings.textureCompression = IsCompressed(entry._level)
                        ? TextureImporterCompression.Compressed
                        : TextureImporterCompression.Uncompressed;
                    defaultSettings.format = IsCompressed(entry._level) ? TextureImporterFormat.Automatic : TextureImporterFormat.RGBA32;
                    importer.SetPlatformTextureSettings(defaultSettings);

                    ApplyPlatform(importer, "Android", _applyAndroidOverride, maxSize, entry._level);
                    ApplyPlatform(importer, "iPhone", _applyIOSOverride, maxSize, entry._level);

                    importer.SaveAndReimport();
                    count++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log($"[TextureAutoCompressor] Đã chỉnh {count} texture.");
        }

        // Bật thì ghi override ASTC/RGBA32 theo mức đã chọn; tắt thì xóa override cũ.
        static void ApplyPlatform(TextureImporter importer, string platform, bool enabled, int maxSize, CompressionLevel level)
        {
            if (!enabled)
            {
                importer.ClearPlatformTextureSettings(platform);
                return;
            }

            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = platform,
                overridden = true,
                maxTextureSize = maxSize,
                format = ToFormat(level),
                resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                textureCompression = IsCompressed(level)
                    ? TextureImporterCompression.Compressed
                    : TextureImporterCompression.Uncompressed
            });
        }

        int ResolveMaxSize(TextureImporter importer, TextureEntry entry)
        {
            if (entry._maxSizeIndex != 0)
                return int.Parse(MaxSizeOptions[entry._maxSizeIndex]);

            // Dùng kích thước NGUỒN: Texture2D.width là kích thước sau import, chạy lại sẽ tự thu nhỏ dần.
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);

            int dimension = _useLowForAutoSize ? Mathf.Min(width, height) : Mathf.Max(width, height);
            return Mathf.Min(GetAdjustedSize(dimension), 1024);
        }

        // =====================================================
        // HELPERS
        // =====================================================

        static bool IsCompressed(CompressionLevel level) => level != CompressionLevel.Uncompressed;

        static TextureImporterFormat ToFormat(CompressionLevel level)
        {
            return level switch
            {
                CompressionLevel.Astc4x4 => TextureImporterFormat.ASTC_4x4,
                CompressionLevel.Astc6x6 => TextureImporterFormat.ASTC_6x6,
                CompressionLevel.Astc8x8 => TextureImporterFormat.ASTC_8x8,
                CompressionLevel.Astc10x10 => TextureImporterFormat.ASTC_10x10,
                CompressionLevel.Astc12x12 => TextureImporterFormat.ASTC_12x12,
                _ => TextureImporterFormat.RGBA32
            };
        }

        static int GetAdjustedSize(int size)
        {
            int[] options = { 32, 64, 128, 256, 512, 1024, 2048 };
            foreach (int opt in options)
            {
                if (size <= opt)
                {
                    if (opt > size + 200)
                    {
                        int index = Array.IndexOf(options, opt);
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
            public CompressionLevel _level;
            public int _maxSizeIndex;
        }
    }
}
#endif
