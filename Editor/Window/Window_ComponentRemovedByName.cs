#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SketchEngine.Editor
{
    public class Window_ComponentRemovedByName : EditorWindow
    {
        string namePattern = "";
        readonly List<Type> selectedTypes = new();
        readonly List<GameObject> matchedObjects = new();

        string typeSearch = "";
        Vector2 scroll;
        List<Type> allComponentTypes;

        [MenuItem("CodeSketch/Tools/Window/Component Search & Removed By Name")]
        public static void ShowWindow()
        {
            GetWindow<Window_ComponentRemovedByName>("Component Cleaner");
        }

        void OnEnable()
        {
            allComponentTypes = GetAllComponentTypes();
        }

        void OnGUI()
        {
            GUILayout.Label("🧹 GameObject Component Cleaner", EditorStyles.boldLabel);

            namePattern = EditorGUILayout.TextField("Tên chứa (Pattern)", namePattern);

            EditorGUILayout.Space();
            GUILayout.Label("🔍 Tìm và chọn Component:", EditorStyles.boldLabel);

            DrawComponentSearch();

            EditorGUILayout.Space();
            GUILayout.Label("🎯 Các Component đã chọn:", EditorStyles.boldLabel);
            DrawSelectedTypeList();

            EditorGUILayout.Space();
            if (GUILayout.Button("🔍 Tìm GameObject phù hợp")) SearchGameObjects();

            if (matchedObjects.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label($"✅ Tìm thấy {matchedObjects.Count} GameObject(s):", EditorStyles.helpBox);

                if (GUILayout.Button("❌ Xoá các Component đã chọn khỏi các GameObject")) RemoveSelectedComponents();

                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(250));
                foreach (GameObject obj in matchedObjects)
                    EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Space(10);
                EditorGUILayout.HelpBox("Chưa tìm thấy GameObject nào phù hợp.", MessageType.Info);
            }
        }

        void DrawComponentSearch()
        {
            EditorGUILayout.BeginHorizontal();
            typeSearch = EditorGUILayout.TextField(typeSearch);

            if (GUILayout.Button("Thêm", GUILayout.Width(60)))
            {
                TryAddTypeFromSearch();
            }
            EditorGUILayout.EndHorizontal();

            // Gợi ý khi đang gõ
            if (!string.IsNullOrWhiteSpace(typeSearch))
            {
                var suggestions = allComponentTypes
                    .Where(t => t.Name.IndexOf(typeSearch, StringComparison.OrdinalIgnoreCase) >= 0)
                    .Take(8);

                foreach (var type in suggestions)
                {
                    if (selectedTypes.Contains(type)) continue;

                    if (GUILayout.Button("➕ " + type.FullName, EditorStyles.miniButton))
                    {
                        selectedTypes.Add(type);
                        typeSearch = "";
                        GUI.FocusControl(null);
                        break;
                    }
                }
            }
        }

        void DrawSelectedTypeList()
        {
            for (int i = 0; i < selectedTypes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(selectedTypes[i].Name, GUILayout.Width(250));
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    selectedTypes.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        void SearchGameObjects()
        {
            matchedObjects.Clear();

            var roots = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (GameObject root in roots)
                Traverse(root.transform);

            Debug.Log($"[Cleaner] Đã tìm thấy {matchedObjects.Count} GameObject phù hợp.");
        }

        void Traverse(Transform t)
        {
            GameObject go = t.gameObject;

            if (string.IsNullOrEmpty(namePattern) || go.name.IndexOf(namePattern, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                bool hasAny = selectedTypes.Any(type => go.GetComponent(type) != null);
                if (hasAny)
                {
                    matchedObjects.Add(go);
                }
            }

            foreach (Transform child in t)
                Traverse(child);
        }

        void RemoveSelectedComponents()
        {
            int count = 0;

            foreach (GameObject go in matchedObjects)
            {
                foreach (Type type in selectedTypes)
                {
                    Component comp = go.GetComponent(type);
                    if (comp != null)
                    {
                        Undo.DestroyObjectImmediate(comp);
                        count++;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[Cleaner] Đã xoá {count} component khỏi {matchedObjects.Count} GameObject.");
            matchedObjects.Clear();
        }

        void TryAddTypeFromSearch()
        {
            var found = allComponentTypes.FirstOrDefault(t =>
                t.Name.Equals(typeSearch, StringComparison.OrdinalIgnoreCase)
                || t.FullName.Equals(typeSearch, StringComparison.OrdinalIgnoreCase));

            if (found != null && !selectedTypes.Contains(found))
            {
                selectedTypes.Add(found);
                typeSearch = "";
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy Component: {typeSearch}");
            }
        }

        List<Type> GetAllComponentTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Type.EmptyTypes; }
                })
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    typeof(Component).IsAssignableFrom(t) &&
                    t.Namespace != null &&
                    !t.Namespace.StartsWith("UnityEditor"))
                .OrderBy(t => t.Name)
                .ToList();
        }
    }
}
#endif
