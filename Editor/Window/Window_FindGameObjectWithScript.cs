#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SketchEngine.Editor
{
    public class Window_FindGameObjectWithScript : EditorWindow
    {
        Type selectedType;
        MonoScript selectedScript;
        readonly List<GameObject> foundObjects = new();

        [MenuItem("CodeSketch/Tools/Window/Find Gameobject With Script")]
        public static void ShowWindow()
        {
            GetWindow<Window_FindGameObjectWithScript>("GameObject With Script");
        }

        void OnGUI()
        {
            GUILayout.Label("Tìm GameObject theo Script", EditorStyles.boldLabel);

            selectedScript =
                (MonoScript)EditorGUILayout.ObjectField("Script", selectedScript, typeof(MonoScript), false);

            if (selectedScript != null && selectedType == null) selectedType = selectedScript.GetClass();

            if (GUILayout.Button("🔍 Tìm GameObject chứa script"))
            {
                if (selectedScript == null || selectedType == null || !selectedType.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    Debug.LogWarning("Script không hợp lệ.");
                    return;
                }

                FindObjectsWithScript(selectedType);
            }

            EditorGUILayout.Space();

            if (foundObjects.Count > 0)
            {
                GUILayout.Label($"Đã tìm thấy {foundObjects.Count} object", EditorStyles.helpBox);

                if (GUILayout.Button("🧹 Gỡ script khỏi các object đã tìm thấy")) RemoveScriptFromObjects(selectedType);

                EditorGUILayout.Space();

                for (var i = 0; i < foundObjects.Count; i++)
                    EditorGUILayout.ObjectField($"[{i}]", foundObjects[i], typeof(GameObject), true);
            }
            else
            {
                EditorGUILayout.HelpBox("Chưa có object nào được tìm.", MessageType.Info);
            }
        }

        void FindObjectsWithScript(Type scriptType)
        {
            foundObjects.Clear();

            var roots = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (GameObject root in roots) TraverseHierarchy(root.transform, scriptType);

            Debug.Log($"[ScriptFinder] Tìm thấy {foundObjects.Count} GameObject có script {scriptType.Name}");
        }

        void TraverseHierarchy(Transform parent, Type scriptType)
        {
            if (parent.GetComponent(scriptType) != null) foundObjects.Add(parent.gameObject);

            foreach (Transform child in parent) TraverseHierarchy(child, scriptType);
        }

        void RemoveScriptFromObjects(Type scriptType)
        {
            var removed = 0;

            foreach (GameObject go in foundObjects)
            {
                Component comp = go.GetComponent(scriptType);
                if (comp != null)
                {
                    Undo.DestroyObjectImmediate(comp);
                    removed++;
                }
            }

            Debug.Log($"[ScriptFinder] Đã xoá {removed} component {scriptType.Name} khỏi scene.");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            foundObjects.Clear();
        }
    }
}
#endif