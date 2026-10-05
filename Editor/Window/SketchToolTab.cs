using System;
using UnityEngine;

namespace SketchEngine.Editor
{
    // Một tab trong Window_SketchTools. Thay cho EditorWindow riêng từng tool.
    public abstract class SketchToolTab
    {
        internal Action RepaintRequest;
        internal Rect Area;

        public abstract string Title { get; }

        public virtual void OnEnable()
        {
        }

        public abstract void OnGUI();

        protected void Repaint()
        {
            RepaintRequest?.Invoke();
        }
    }
}
