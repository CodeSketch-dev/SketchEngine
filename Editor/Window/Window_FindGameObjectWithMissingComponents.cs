#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SketchEngine.Editor
{
    public class Window_FindGameObjectWithMissingComponents : EditorWindow
    {
        enum SearchScope
        {
            ActiveScene,
            SelectedObjects
        }

        // ================= OPTIONS =================
        SearchScope scope = SearchScope.ActiveScene;

        bool includeInactive = true;
        bool includeChildren = true;
        bool includePrefabs = false;

        int previewObjectCount;
        int previewMissingCount;

        GUIStyle _boxStyle;

        [MenuItem("CodeSketch/Tools/Window/Find Missing Components")]
        public static void ShowWindow()
        {
            GetWindow<Window_FindGameObjectWithMissingComponents>(
                "Missing Components"
            );
        }

        void OnEnable()
        {
            _boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8)
            };
        }

        void OnGUI()
        {
            DrawHeader();
            DrawSearchOptions();
            DrawPreviewSection();
            DrawActions();
        }

        // =====================================================
        // UI SECTIONS
        // =====================================================

        void DrawHeader()
        {
            GUILayout.Space(4);
            GUILayout.Label(
                "Find GameObjects With Missing Components",
                EditorStyles.boldLabel
            );

            EditorGUILayout.HelpBox(
                "This tool helps you find, select, or remove missing scripts\n" +
                "from GameObjects in the scene or selected hierarchy.",
                MessageType.Info
            );
        }

        void DrawSearchOptions()
        {
            GUILayout.Space(6);
            GUILayout.BeginVertical(_boxStyle);

            GUILayout.Label("Search Scope", EditorStyles.boldLabel);
            scope = (SearchScope)EditorGUILayout.EnumPopup("Scope", scope);

            GUILayout.Space(4);
            GUILayout.Label("Include Options", EditorStyles.boldLabel);

            includeInactive = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    "Include Inactive GameObjects",
                    "Search inactive GameObjects as well"
                ),
                includeInactive
            );

            includeChildren = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    "Include Children",
                    "Search all children recursively"
                ),
                includeChildren
            );

            includePrefabs = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    "Include Prefab Sources",
                    "Also scan prefab source objects (advanced)"
                ),
                includePrefabs
            );

            GUILayout.EndVertical();
        }

        void DrawPreviewSection()
        {
            GUILayout.Space(6);
            GUILayout.BeginVertical(_boxStyle);

            GUILayout.Label("Preview", EditorStyles.boldLabel);

            if (GUILayout.Button("Scan Preview", GUILayout.Height(22)))
            {
                ScanPreview();
            }

            EditorGUILayout.LabelField(
                "GameObjects scanned",
                previewObjectCount.ToString()
            );
            EditorGUILayout.LabelField(
                "Missing scripts found",
                previewMissingCount.ToString()
            );

            GUILayout.EndVertical();
        }

        void DrawActions()
        {
            GUILayout.Space(8);
            GUILayout.BeginVertical(_boxStyle);

            GUILayout.Label("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Log Missing Scripts"))
                LogMissingScripts(GetTargetObjects());

            if (GUILayout.Button("Select GameObjects"))
                SelectGameObjectsWithMissingScripts(GetTargetObjects());

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Remove Missing Scripts", GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog(
                    "Remove Missing Scripts",
                    "This will permanently remove missing scripts.\n\nAre you sure?",
                    "Remove",
                    "Cancel"))
                {
                    RemoveMissingScripts(GetTargetObjects());
                }
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndVertical();
        }

        // =====================================================
        // CORE LOGIC
        // =====================================================

        GameObject[] GetTargetObjects()
        {
            if (scope == SearchScope.ActiveScene)
            {
                return SceneManager
                    .GetActiveScene()
                    .GetRootGameObjects();
            }

            return SelectedGameObjects(
                includeInactive,
                includeChildren,
                includePrefabs
            );
        }

        void ScanPreview()
        {
            previewObjectCount = 0;
            previewMissingCount = 0;

            foreach (var go in GetTargetObjects())
            {
                previewObjectCount++;
                previewMissingCount += RecursiveMissingScriptCount(go);
            }
        }

        public static void LogMissingScripts(GameObject[] gameObjects)
        {
            int missing = 0;
            foreach (var go in gameObjects)
                missing += RecursiveMissingScriptCount(go);

            Debug.Log(
                $"[Missing Scripts] Found {missing} missing scripts " +
                $"in {gameObjects.Length} GameObjects."
            );
        }

        public static void SelectGameObjectsWithMissingScripts(GameObject[] gameObjects)
        {
            var selections = new List<GameObject>();

            foreach (var go in gameObjects)
                if (RecursiveMissingScriptCount(go) > 0)
                    selections.Add(go);

            Selection.objects = selections.ToArray();
        }

        public static void RemoveMissingScripts(GameObject[] gameObjects)
        {
            int removed = 0;

            foreach (var go in gameObjects)
            {
                int count = GameObjectUtility
                    .GetMonoBehavioursWithMissingScriptCount(go);

                if (count > 0)
                {
                    Undo.RegisterCompleteObjectUndo(
                        go,
                        "Remove Missing Scripts"
                    );

                    GameObjectUtility
                        .RemoveMonoBehavioursWithMissingScript(go);

                    removed += count;
                }
            }

            Debug.Log(
                $"[Missing Scripts] Removed {removed} missing scripts."
            );
        }

        // =====================================================
        // UTILITIES
        // =====================================================

        static int RecursiveMissingScriptCount(GameObject gameObject)
        {
            int count =
                GameObjectUtility
                    .GetMonoBehavioursWithMissingScriptCount(gameObject);

            var children =
                gameObject.GetComponentsInChildren<Transform>(true);

            foreach (var t in children)
            {
                if (t == gameObject.transform)
                    continue;

                count +=
                    GameObjectUtility
                        .GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            }

            return count;
        }

        static GameObject[] SelectedGameObjects(
            bool includeInactive,
            bool includeChildren,
            bool includePrefabs
        )
        {
            var results = new HashSet<GameObject>();

            foreach (var root in Selection.gameObjects)
            {
                results.Add(root);

                if (includeChildren)
                {
                    var children =
                        root.GetComponentsInChildren<Transform>(
                            includeInactive
                        );

                    foreach (var t in children)
                        results.Add(t.gameObject);
                }

                if (includePrefabs)
                {
                    var prefabs = new HashSet<GameObject>();
                    CollectPrefabSources(root, prefabs);
                    results.UnionWith(prefabs);
                }
            }

            return new List<GameObject>(results).ToArray();
        }

        static void CollectPrefabSources(
            GameObject instance,
            HashSet<GameObject> prefabs
        )
        {
            var source =
                PrefabUtility.GetCorrespondingObjectFromSource(instance);

            if (source == null || !prefabs.Add(source))
                return;

            CollectPrefabSources(source, prefabs);
        }
    }
}
#endif
