using System.Runtime.CompilerServices;
using SketchEngine.Mono;
using UnityEngine;
using UnityEngine.UI;

namespace SketchEngine.Core.UI
{
    public class UIButtonBase : MonoCached
    {
        [SerializeField, HideInInspector] Button _button;

        public Button Button
        {
            get
            {
                if (_button == null)
                    _button = GetComponentInChildren<Button>(false);

                return _button;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EnsureButton()
        {
            if (_button == null) _button = Button;
        }

        protected virtual void Awake()
        {
            EnsureButton();
            Button.onClick.AddListener(Button_OnClick);
        }

        public virtual void Button_OnClick()
        {
            // for override
        }

        protected virtual void OnValidate()
        {
            EnsureButton();
        }
    }
}