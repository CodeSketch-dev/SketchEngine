using System;
using System.Linq;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace SketchEngine.Editor.Scriptable
{
    internal class EndNameEdit : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
        }
    }

    /// <summary>
    /// Cửa sổ hiển thị các ScriptableObject tìm được.
    /// Khi trùng tên class thì hiển thị kèm namespace, và file tạo ra cũng mang namespace để không lẫn.
    /// </summary>
    public class ScriptableObjectWindow : EditorWindow
    {
        string _strSearch = "";
        Vector2 _scrollPosition;

        string[] _names;
        Type[] _types;

        bool _focused;

        public Type[] Types
        {
            get => _types;
            set
            {
                _types = value ?? Array.Empty<Type>();

                var duplicateCounts = _types
                    .GroupBy(type => type.Name)
                    .ToDictionary(group => group.Key, group => group.Count());

                _names = _types
                    .Select(type => duplicateCounts[type.Name] > 1
                        ? GetQualifiedName(type)
                        : type.Name)
                    .ToArray();
            }
        }

        static string GetQualifiedName(Type type)
        {
            string namespaceName = type.Namespace;
            if (string.IsNullOrEmpty(namespaceName))
                namespaceName = type.Assembly.GetName().Name;

            return $"{namespaceName}.{type.Name}";
        }

        static string GetAssetFileName(Type type)
        {
            return string.IsNullOrEmpty(type.Namespace)
                ? $"{type.Name}.asset"
                : $"{type.Namespace}.{type.Name}.asset";
        }

        public void OnGUI()
        {
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar") ?? GUI.skin.box);

            GUI.SetNextControlName("SearchBar");
            _strSearch = GUILayout.TextField(_strSearch, GUI.skin.FindStyle("ToolbarSeachTextField") ?? GUI.skin.textField);

            GUILayout.EndHorizontal();

            if (_types == null)
                return;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

            for (int i = 0; i < _types.Length; i++)
            {
                if (_strSearch != "" && _names[i].IndexOf(_strSearch, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_names[i]))
                {
                    var asset = CreateInstance(_types[i]);
                    ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                        asset.GetInstanceID(),
                        CreateInstance<EndNameEdit>(),
                        GetAssetFileName(_types[i]),
                        AssetPreview.GetMiniThumbnail(asset),
                        null);

                    Close();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            if (!_focused)
            {
                GUI.FocusControl("SearchBar");
                _focused = true;
            }
        }
    }
}
