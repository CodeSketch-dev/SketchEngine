using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SketchEngine.Editor
{
    public class Window_GameObjectReplaceByOther : SketchToolTab
    {
        public override string Title => "GameObject Replace By Other";

        readonly List<GameObject> objectsToCloneFrom = new(); // Danh sách các object cần clone
        
        GameObject _parentObject; // GameObject cha
        GameObject _prefabToClone; // Prefab cần clone
        Vector2 _scrollPosition; // Vị trí scroll

        public override void OnGUI()
        {
            GUILayout.Label("Select Parent Object", EditorStyles.boldLabel);
            _parentObject = (GameObject)EditorGUILayout.ObjectField("Spawn To", _parentObject, typeof(GameObject), true);

            GUILayout.Label("Select Prefab to Clone", EditorStyles.boldLabel);
            _prefabToClone = (GameObject)EditorGUILayout.ObjectField("Prefab", _prefabToClone, typeof(GameObject), false);

            GUILayout.Label("Drag GameObjects Here", EditorStyles.boldLabel);

            // Vùng Drag & Drop
            Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drop GameObjects Here", EditorStyles.helpBox);
            HandleDragAndDrop(dropArea);

            if (GUILayout.Button("Add Selected Objects"))
            {
                foreach (GameObject obj in Selection.gameObjects)
                    if (!objectsToCloneFrom.Contains(obj))
                        objectsToCloneFrom.Add(obj);
            }

            GUILayout.Space(5);

            // ScrollView để tránh tràn UI khi có nhiều Object
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));

            if (objectsToCloneFrom.Count > 0)
            {
                GUILayout.Label("Objects to Clone:", EditorStyles.boldLabel);
                for (int i = 0; i < objectsToCloneFrom.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    objectsToCloneFrom[i] = (GameObject)EditorGUILayout.ObjectField(objectsToCloneFrom[i], typeof(GameObject), true);
                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        objectsToCloneFrom.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("No objects selected.", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView(); // Kết thúc ScrollView

            GUILayout.Space(10);

            // Nút "Clean" để xóa toàn bộ danh sách Object
            if (GUILayout.Button("Clean List"))
            {
                objectsToCloneFrom.Clear();
            }

            GUILayout.Space(5);

            // Button Clone
            GUI.enabled = _parentObject != null && _prefabToClone != null && objectsToCloneFrom.Count > 0;
            if (GUILayout.Button("Clone Prefab as Childs")) ClonePrefabs();
            GUI.enabled = true;
        }

        void HandleDragAndDrop(Rect dropArea)
        {
            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is GameObject go && !objectsToCloneFrom.Contains(go))
                            {
                                objectsToCloneFrom.Add(go);
                            }
                        }
                        Event.current.Use();
                    }
                }
            }
        }

        void ClonePrefabs()
        {
            if (_parentObject == null)
            {
                Debug.LogError("Parent Object is not assigned!");
                return;
            }

            if (_prefabToClone == null)
            {
                Debug.LogError("Prefab to Clone is not assigned!");
                return;
            }

            if (objectsToCloneFrom.Count == 0)
            {
                Debug.LogError("No objects selected to clone from!");
                return;
            }

            foreach (GameObject obj in objectsToCloneFrom)
            {
                if (obj == null) continue;

                // Tạo bản sao của Prefab như một GameObject con của `parentObject`
                GameObject newClone = PrefabUtility.InstantiatePrefab(_prefabToClone) as GameObject;
                if (newClone == null)
                {
                    Debug.LogError($"Failed to instantiate prefab: {_prefabToClone.name}");
                    continue;
                }

                // Set parent
                newClone.transform.SetParent(_parentObject.transform);

                // Copy transform từ object gốc
                newClone.transform.position = obj.transform.position;
                newClone.transform.rotation = obj.transform.rotation;
                newClone.transform.localScale = obj.transform.lossyScale;

                Debug.Log($"Cloned Prefab as Child: {newClone.name} under {_parentObject.name}");
            }

            // Nếu `parentObject` là một Prefab, lưu lại Prefab
            if (PrefabUtility.IsPartOfPrefabAsset(_parentObject))
            {
                var prefabPath = AssetDatabase.GetAssetPath(_parentObject);
                PrefabUtility.SaveAsPrefabAsset(_parentObject, prefabPath);
                Debug.Log($"Updated Prefab: {_parentObject.name} saved at {prefabPath}");
            }

            AssetDatabase.Refresh();
        }
    }
}
