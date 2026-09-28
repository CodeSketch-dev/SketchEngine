using System;
using System.Linq;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace SketchEngine.Editor.Scriptable
{
    internal class EndNameEdit : EndNameEditAction
    {
        #region Implemented abstract members of EndNameEditAciton

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
        }

        #endregion
    }
    
    /// <summary>
    /// Scriptable object window.
    /// Cửa sổ hiển thị các ScriptableObject đã tìm được trong Project
    /// </summary>
    public class ScriptableObjectWindow : EditorWindow
    {
        string _strSearch = "";
        Vector2 _scrollPosition;

        string[] _names;
        Type[] _types;

        bool _focused = false;
        
        public Type[] Types
        {
            get => _types;
            set
            {
                _types = value;
                _names = _types.Select(t => t.FullName).ToArray();
            }
        }
        
        public void OnGUI()
        {
            //Search bar
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar") ?? GUI.skin.box);

            GUI.SetNextControlName("SearchBar");

            _strSearch = GUILayout.TextField(_strSearch, GUI.skin.FindStyle("ToolbarSeachTextField") ?? GUI.skin.textField);

            GUILayout.EndHorizontal();

            if (_types == null)
                return;

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);

            for (int i = 0; i < _types.Length; i++)
            {
                if (_strSearch != "" && _types[i].Name.IndexOf(_strSearch, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(_types[i].Name))
                {
                    var asset = CreateInstance(_types[i]);
                    ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                        asset.GetInstanceID(),
                        CreateInstance<EndNameEdit>(),
                        $"{_names[i]}.asset",
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
