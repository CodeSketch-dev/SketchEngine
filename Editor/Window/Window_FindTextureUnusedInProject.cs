#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SketchEngine.Editor
{
    public class Window_FindTextureUnusedInProject : EditorWindow
    {
        const float ITEM_HEIGHT  = 38f;
        const float PREVIEW_SIZE = 32f;

        readonly List<string> unusedTextures = new();
        readonly Dictionary<string, Texture> previewCache = new();

        ReorderableList reorderableList;
        string selectedFolder = "Assets";
        Vector2 scroll;

        // =====================================================
        // MENU
        // =====================================================

        [MenuItem("CodeSketch/Tools/Texture/Find Unused Textures")]
        static void Open()
        {
            GetWindow<Window_FindTextureUnusedInProject>("Unused Textures");
        }

        // =====================================================
        // GUI
        // =====================================================

        void OnGUI()
        {
            GUILayout.Space(4);
            GUILayout.Label("Scan Folder", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(selectedFolder);
            if (GUILayout.Button("Select", GUILayout.Width(80)))
                SelectFolder();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6);

            if (GUILayout.Button("Scan Unused Textures", GUILayout.Height(28)))
            {
                FindUnusedTextures(selectedFolder);
                BuildList();
            }

            if (unusedTextures.Count > 0)
            {
                GUILayout.Space(6);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Move ALL to Folder", GUILayout.Height(26)))
                    MoveAllUnused();

                if (GUILayout.Button("Refresh", GUILayout.Height(26)))
                {
                    FindUnusedTextures(selectedFolder);
                    reorderableList.list = unusedTextures;
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(6);

            if (reorderableList != null)
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                reorderableList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }
        }

        // =====================================================
        // SCAN LOGIC (FAST)
        // =====================================================

        void FindUnusedTextures(string folder)
        {
            unusedTextures.Clear();
            previewCache.Clear();

            string[] texGuids = AssetDatabase.FindAssets("t:Texture", new[] { folder });
            HashSet<string> texturePaths = new();

            foreach (var guid in texGuids)
                texturePaths.Add(AssetDatabase.GUIDToAssetPath(guid));

            string[] assetGuids = AssetDatabase.FindAssets("t:Prefab t:Scene", new[] { folder });
            HashSet<string> usedTextures = new();

            for (int i = 0; i < assetGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                string[] deps = AssetDatabase.GetDependencies(assetPath, true);

                foreach (var dep in deps)
                {
                    if (texturePaths.Contains(dep))
                        usedTextures.Add(dep);
                }

                if (i % 25 == 0)
                {
                    EditorUtility.DisplayProgressBar(
                        "Scanning Dependencies",
                        assetPath,
                        (float)i / assetGuids.Length
                    );
                }
            }

            EditorUtility.ClearProgressBar();

            foreach (var tex in texturePaths)
            {
                if (!usedTextures.Contains(tex))
                    unusedTextures.Add(tex);
            }

            Debug.Log($"[UnusedTexture] Found {unusedTextures.Count} unused textures");
        }

        // =====================================================
        // MOVE
        // =====================================================

        void MoveAllUnused()
        {
            if (unusedTextures.Count == 0)
                return;

            string targetFolder = EditorUtility.OpenFolderPanel(
                "Select Target Folder",
                "Assets",
                ""
            );

            if (string.IsNullOrEmpty(targetFolder))
                return;

            if (!targetFolder.StartsWith(Application.dataPath))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Folder",
                    "Target folder must be inside Assets/",
                    "OK"
                );
                return;
            }

            string assetTarget = "Assets" + targetFolder.Replace(Application.dataPath, "");

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var path in unusedTextures)
                {
                    string fileName = System.IO.Path.GetFileName(path);
                    string destPath = AssetDatabase.GenerateUniqueAssetPath(
                        $"{assetTarget}/{fileName}"
                    );

                    AssetDatabase.MoveAsset(path, destPath);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            unusedTextures.Clear();
            previewCache.Clear();
            reorderableList.list = unusedTextures;
        }

        // =====================================================
        // LIST UI (STABLE + CLICKABLE)
        // =====================================================

        void BuildList()
        {
            reorderableList = new ReorderableList(
                unusedTextures,
                typeof(string),
                false, true, false, false
            );

            reorderableList.elementHeight = ITEM_HEIGHT;

            reorderableList.drawHeaderCallback =
                rect => EditorGUI.LabelField(rect, "Unused Textures");

            reorderableList.drawElementCallback = (rect, index, _, __) =>
            {
                rect.y += 2;

                string path = unusedTextures[index];
                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path);

                // ---- PREVIEW CACHE ----
                if (!previewCache.TryGetValue(path, out var preview))
                {
                    preview = AssetPreview.GetAssetPreview(tex)
                              ?? AssetPreview.GetMiniThumbnail(tex);
                    previewCache[path] = preview;
                }

                Rect previewRect = new Rect(
                    rect.x,
                    rect.y,
                    PREVIEW_SIZE,
                    PREVIEW_SIZE
                );

                if (preview)
                    GUI.DrawTexture(previewRect, preview, ScaleMode.ScaleToFit);

                Rect labelRect = new Rect(
                    rect.x + PREVIEW_SIZE + 6,
                    rect.y + 8,
                    rect.width - PREVIEW_SIZE - 6,
                    EditorGUIUtility.singleLineHeight
                );

                EditorGUI.LabelField(labelRect, path, EditorStyles.miniLabel);

                // ---- CLICK HANDLING ----
                if (Event.current.type == EventType.MouseDown &&
                    rect.Contains(Event.current.mousePosition))
                {
                    Selection.activeObject = tex;

                    if (Event.current.clickCount == 2)
                        EditorGUIUtility.PingObject(tex);

                    Event.current.Use();
                }
            };
        }

        // =====================================================
        // HELPERS
        // =====================================================

        void SelectFolder()
        {
            string folder = EditorUtility.OpenFolderPanel(
                "Select Scan Folder",
                "Assets",
                ""
            );

            if (!string.IsNullOrEmpty(folder))
                selectedFolder = "Assets" + folder.Replace(Application.dataPath, "");
        }
    }
}
#endif
