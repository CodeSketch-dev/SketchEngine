#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SketchEngine.Editor
{
    public class Window_FindGameObjectWithTag : EditorWindow
    {
        // =====================================================
        // DATA
        // =====================================================

        readonly List<GameObject> _foundObjects = new List<GameObject>();

        string _tagToSearch = "";
        string _prefabSearchFolder = "Assets";

        ReorderableList _list;
        Vector2 _scrollPos;

        // =====================================================
        // MENU
        // =====================================================

        [MenuItem("CodeSketch/Tools/Window/Find GameObject With Tag")]
        public static void ShowWindow()
        {
            GetWindow<Window_FindGameObjectWithTag>("Find GameObject With Tag");
        }

        // =====================================================
        // INIT
        // =====================================================

        void OnEnable()
        {
            _list = new ReorderableList(_foundObjects, typeof(GameObject), true, true, false, false)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Found GameObjects");
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (index < 0 || index >= _foundObjects.Count)
                        return;

                    GameObject obj = _foundObjects[index];

                    EditorGUI.ObjectField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        obj,
                        typeof(GameObject),
                        true
                    );
                }
            };
        }

        // =====================================================
        // GUI
        // =====================================================

        void OnGUI()
        {
            GUILayout.Label("Tag Finder", EditorStyles.boldLabel);

            _tagToSearch = EditorGUILayout.TextField("Tag to Search", _tagToSearch);

            EditorGUILayout.Space(5);

            DrawPrefabSearchSection();

            EditorGUILayout.Space(10);

            DrawResultList();
        }

        void DrawPrefabSearchSection()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Prefab Search", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Folder", GUILayout.Width(50));
            EditorGUILayout.SelectableLabel(
                _prefabSearchFolder,
                EditorStyles.textField,
                GUILayout.Height(EditorGUIUtility.singleLineHeight)
            );

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                SelectPrefabFolder();
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Find In Scene"))
            {
                FindInScene();
            }

            if (GUILayout.Button("Find In Prefabs"))
            {
                FindInPrefabs();
            }

            EditorGUILayout.EndVertical();
        }

        void DrawResultList()
        {
            if (_foundObjects.Count == 0)
            {
                GUILayout.Label("No GameObjects found.");
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            _list.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }

        // =====================================================
        // SEARCH LOGIC
        // =====================================================

        void FindInScene()
        {
            _foundObjects.Clear();

            if (!IsValidTag(_tagToSearch))
            {
                ShowInvalidTagDialog();
                return;
            }

            GameObject[] sceneObjects = GameObject.FindGameObjectsWithTag(_tagToSearch);
            _foundObjects.AddRange(sceneObjects);
        }

        void FindInPrefabs()
        {
            _foundObjects.Clear();

            if (!IsValidTag(_tagToSearch))
            {
                ShowInvalidTagDialog();
                return;
            }

            string[] searchFolders = { _prefabSearchFolder };
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", searchFolders);

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                    continue;

                FindTaggedChildren(prefab.transform);
            }
        }

        void FindTaggedChildren(Transform parent)
        {
            if (parent.CompareTag(_tagToSearch))
            {
                _foundObjects.Add(parent.gameObject);
            }

            foreach (Transform child in parent)
            {
                FindTaggedChildren(child);
            }
        }

        // =====================================================
        // FOLDER PICKER
        // =====================================================

        void SelectPrefabFolder()
        {
            string absPath = EditorUtility.OpenFolderPanel(
                "Select Prefab Folder",
                Application.dataPath,
                ""
            );

            if (string.IsNullOrEmpty(absPath))
                return;

            absPath = absPath.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');

            if (!absPath.StartsWith(dataPath))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Folder",
                    "Please select a folder inside Assets.",
                    "OK"
                );
                return;
            }

            _prefabSearchFolder = "Assets" + absPath.Substring(dataPath.Length);
        }

        // =====================================================
        // UTIL
        // =====================================================

        static bool IsValidTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return false;

            try
            {
                GameObject.FindGameObjectWithTag(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static void ShowInvalidTagDialog()
        {
            EditorUtility.DisplayDialog(
                "Invalid Tag",
                "The tag does not exist in Tag Manager.",
                "OK"
            );
        }
    }
}
#endif
