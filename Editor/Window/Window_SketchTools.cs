using UnityEditor;
using UnityEngine;

namespace SketchEngine.Editor
{
    public class Window_SketchTools : EditorWindow
    {
        SketchToolTab[] _tabs;
        string[] _titles;
        bool[] _tabEnabled;
        int _selected;
        GUIStyle _tabButtonStyle;

        [MenuItem("SketchEngine/Tools/Window/Sketch Tools")]
        public static void ShowWindow()
        {
            GetWindow<Window_SketchTools>("Sketch Tools");
        }

        void OnEnable()
        {
            _tabs = new SketchToolTab[]
            {
                new Window_FindAndClean(),
                new Window_FindGameObjectWithLayerMask(),
                new Window_FindGameObjectWithMissingComponents(),
                new Window_FindTextureUnusedInProject(),
                new Window_GameObjectReplaceByOther(),
                new Window_SpriteToPNG(),
                new Window_TextureAutoCompressor(),
            };

            _titles = new string[_tabs.Length];
            _tabEnabled = new bool[_tabs.Length];

            for (int i = 0; i < _tabs.Length; i++)
            {
                _titles[i] = _tabs[i].Title;
                _tabs[i].RepaintRequest = Repaint;
            }

            _selected = Mathf.Clamp(_selected, 0, _tabs.Length - 1);
        }

        void OnGUI()
        {
            const float MinTabWidth = 160f;
            int columns = Mathf.Max(1, Mathf.FloorToInt(position.width / MinTabWidth));

            _tabButtonStyle ??= new GUIStyle(EditorStyles.toolbarButton)
            {
                wordWrap = true,
                fixedHeight = 0f,
                padding = new RectOffset(6, 6, 4, 4),
            };

            _selected = GUILayout.SelectionGrid(_selected, _titles, columns, _tabButtonStyle);

            SketchToolTab tab = _tabs[_selected];

            // Chỉ gọi OnEnable của tab khi người dùng mở tab đó lần đầu, tránh quét scene/project lúc mở window.
            if (!_tabEnabled[_selected])
            {
                _tabEnabled[_selected] = true;
                tab.OnEnable();
            }

            tab.Area = position;
            tab.OnGUI();
        }
    }
}
