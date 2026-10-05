#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SketchEngine.Editor
{
    public class Window_FindAndClean : SketchToolTab
    {
        public override string Title => "Find";

        string _namePattern = "";
        int _tagIndex;
        string[] _tagOptions;

        string _typeSearch = "";
        readonly List<Type> _selectedTypes = new List<Type>();
        List<Type> _allComponentTypes;

        readonly List<GameObject> _results = new List<GameObject>();
        Vector2 _resultScroll;
        GUIStyle _rowStyle;

        public override void OnEnable()
        {
            _allComponentTypes = GetAllComponentTypes();
            _tagOptions = BuildTagOptions();
            _rowStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
        }

        public override void OnGUI()
        {
            GUILayout.Label("Tìm GameObject", EditorStyles.boldLabel);

            _namePattern = EditorGUILayout.TextField("Tên chứa (Pattern)", _namePattern);
            _tagIndex = EditorGUILayout.Popup("Tag", _tagIndex, _tagOptions);

            EditorGUILayout.Space();
            GUILayout.Label("Component (bất kỳ cái nào trong danh sách):", EditorStyles.boldLabel);
            DrawComponentSearch();
            DrawSelectedTypeList();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Find", GUILayout.Height(24)))
                Find();

            using (new EditorGUI.DisabledScope(_results.Count == 0 || _selectedTypes.Count == 0))
            {
                if (GUILayout.Button("🧹 Clean selected components", GUILayout.Height(24)))
                    Clean();
            }
            EditorGUILayout.EndHorizontal();

            DrawResults();
        }

        // =====================================================
        // FIND
        // =====================================================

        void Find()
        {
            _results.Clear();

            string tag = _tagIndex > 0 ? _tagOptions[_tagIndex] : null;
            bool hasName = !string.IsNullOrEmpty(_namePattern);

            if (!hasName && tag == null && _selectedTypes.Count == 0)
            {
                Debug.LogWarning("[Find] Nhập ít nhất một điều kiện: tên, tag hoặc component.");
                return;
            }

            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                Traverse(root.transform, hasName, tag);

            Repaint();
        }

        void Traverse(Transform t, bool hasName, string tag)
        {
            GameObject go = t.gameObject;

            bool nameOk = !hasName || go.name.IndexOf(_namePattern, StringComparison.OrdinalIgnoreCase) >= 0;
            bool tagOk = tag == null || go.CompareTag(tag);
            bool typeOk = _selectedTypes.Count == 0 || _selectedTypes.Any(type => go.GetComponent(type) != null);

            if (nameOk && tagOk && typeOk)
                _results.Add(go);

            foreach (Transform child in t)
                Traverse(child, hasName, tag);
        }

        // =====================================================
        // CLEAN
        // =====================================================

        void Clean()
        {
            if (_selectedTypes.Count == 0)
                return;

            int removed = 0;

            foreach (var go in _results)
            {
                if (go == null) continue;

                foreach (var type in _selectedTypes)
                {
                    Component component = go.GetComponent(type);
                    if (component != null)
                    {
                        Undo.DestroyObjectImmediate(component);
                        removed++;
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[Find] Đã xoá {removed} component khỏi {_results.Count} GameObject.");

            Find();
        }

        // =====================================================
        // UI
        // =====================================================

        void DrawComponentSearch()
        {
            EditorGUILayout.BeginHorizontal();
            _typeSearch = EditorGUILayout.TextField(_typeSearch);
            if (GUILayout.Button("Thêm", GUILayout.Width(60)))
                TryAddTypeFromSearch();
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrWhiteSpace(_typeSearch))
                return;

            var suggestions = _allComponentTypes
                .Where(t => t.Name.IndexOf(_typeSearch, StringComparison.OrdinalIgnoreCase) >= 0 && !_selectedTypes.Contains(t))
                .Take(8)
                .ToList();

            foreach (var type in suggestions)
            {
                if (GUILayout.Button("➕ " + type.FullName, EditorStyles.miniButton))
                {
                    _selectedTypes.Add(type);
                    _typeSearch = "";
                    GUI.FocusControl(null);
                    break;
                }
            }
        }

        void DrawSelectedTypeList()
        {
            for (int i = 0; i < _selectedTypes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(_selectedTypes[i].Name, GUILayout.Width(250));
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    _selectedTypes.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawResults()
        {
            EditorGUILayout.Space();
            GUILayout.Label($"Kết quả: {_results.Count} GameObject", EditorStyles.boldLabel);

            _resultScroll = EditorGUILayout.BeginScrollView(_resultScroll, GUILayout.Height(250));

            for (int i = 0; i < _results.Count; i++)
            {
                GameObject go = _results[i];
                if (go == null) continue;

                if (GUILayout.Button(GetHierarchyPath(go.transform), _rowStyle))
                {
                    Selection.activeGameObject = go;
                    EditorGUIUtility.PingObject(go);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        void TryAddTypeFromSearch()
        {
            var found = _allComponentTypes.FirstOrDefault(t =>
                t.Name.Equals(_typeSearch, StringComparison.OrdinalIgnoreCase)
                || t.FullName.Equals(_typeSearch, StringComparison.OrdinalIgnoreCase));

            if (found != null && !_selectedTypes.Contains(found))
            {
                _selectedTypes.Add(found);
                _typeSearch = "";
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy Component: {_typeSearch}");
            }
        }

        // =====================================================
        // UTILITIES
        // =====================================================

        static string[] BuildTagOptions()
        {
            string[] tags = InternalEditorUtility.tags;
            var options = new string[tags.Length + 1];
            options[0] = "(Any)";
            Array.Copy(tags, 0, options, 1, tags.Length);
            return options;
        }

        static string GetHierarchyPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = $"{t.name}/{path}";
            }
            return path;
        }

        static List<Type> GetAllComponentTypes()
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
                    (t.Namespace == null || !t.Namespace.StartsWith("UnityEditor")))
                .OrderBy(t => t.Name)
                .ToList();
        }
    }
}
#endif
