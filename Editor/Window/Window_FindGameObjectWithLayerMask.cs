#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.IO;

namespace SketchEngine.Editor
{
    public class Window_FindGameObjectWithLayerMask : EditorWindow
    {
        enum SearchScope
        {
            Scene,
            PrefabsInFolder
        }

        string layerToSearch = "Default";
        SearchScope searchScope = SearchScope.Scene;

        string searchFolderPath = "Assets";

        readonly List<GameObject> foundObjects = new List<GameObject>();
        ReorderableList reorderableList;
        Vector2 scrollPos;

        [MenuItem("CodeSketch/Tools/Window/Find GameObject With Layer")]
        public static void ShowWindow()
        {
            GetWindow<Window_FindGameObjectWithLayerMask>("Layer Finder");
        }

        void OnEnable()
        {
            reorderableList = new ReorderableList(foundObjects, typeof(GameObject), true, true, false, false)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Found GameObjects");
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    foundObjects[index] = (GameObject)EditorGUI.ObjectField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        foundObjects[index],
                        typeof(GameObject),
                        true
                    );
                }
            };
        }

        void OnGUI()
        {
            GUILayout.Label("Layer Finder", EditorStyles.boldLabel);
            GUILayout.Space(4);

            // Layer
            layerToSearch = EditorGUILayout.TextField("Layer to Search", layerToSearch);

            // Scope
            searchScope = (SearchScope)EditorGUILayout.EnumPopup("Search Scope", searchScope);

            // Folder selection
            if (searchScope == SearchScope.PrefabsInFolder)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.TextField("Search Folder", searchFolderPath);

                if (GUILayout.Button("Select...", GUILayout.Width(80)))
                {
                    SelectFolder();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox(
                    $"Searching prefabs in:\n{searchFolderPath}",
                    MessageType.None
                );
            }

            GUILayout.Space(6);

            if (GUILayout.Button("Find GameObjects", GUILayout.Height(26)))
            {
                FindGameObjectsWithLayer();
            }

            GUILayout.Space(6);

            if (foundObjects.Count > 0)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                reorderableList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"No GameObjects found with layer: {layerToSearch}",
                    MessageType.Info
                );
            }
        }

        // =====================================================
        // FOLDER PICKER
        // =====================================================

        void SelectFolder()
        {
            string absPath = EditorUtility.OpenFolderPanel(
                "Select Folder Containing Prefabs",
                Application.dataPath,
                ""
            );

            if (string.IsNullOrEmpty(absPath))
                return;

            if (!absPath.Replace("\\", "/").StartsWith(Application.dataPath))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Folder",
                    "Please select a folder inside the Assets directory.",
                    "OK"
                );
                return;
            }

            // Convert absolute path → Assets/...
            searchFolderPath = "Assets" + absPath.Substring(Application.dataPath.Length);
        }

        // =====================================================
        // FIND LOGIC
        // =====================================================

        void FindGameObjectsWithLayer()
        {
            foundObjects.Clear();

            int layer = LayerMask.NameToLayer(layerToSearch);
            if (layer == -1)
            {
                Debug.LogWarning("Layer not found: " + layerToSearch);
                return;
            }

            if (searchScope == SearchScope.Scene)
                FindInScene(layer);
            else
                FindInPrefabs(layer);
        }

        void FindInScene(int layer)
        {
            var sceneObjects = FindObjectsOfType<GameObject>(true);
            foreach (var obj in sceneObjects)
            {
                if (obj.layer == layer)
                    foundObjects.Add(obj);
            }
        }

        void FindInPrefabs(int layer)
        {
            string[] folders = { searchFolderPath };
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", folders);

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    FindLayeredChildren(prefab.transform, layer);
                }
            }
        }

        void FindLayeredChildren(Transform parent, int layer)
        {
            if (parent.gameObject.layer == layer)
                foundObjects.Add(parent.gameObject);

            foreach (Transform child in parent)
                FindLayeredChildren(child, layer);
        }
    }
}
#endif
