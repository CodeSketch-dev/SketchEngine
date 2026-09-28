using System.Runtime.CompilerServices;
using SketchEngine.Mono;
using TMPro;
using UnityEngine;

namespace SketchEngine.Core.Text
{
    public class UITextWorldCanvas : MonoBase
    {
        [SerializeField, HideInInspector] TextMeshProUGUI _text;
        [SerializeField, HideInInspector] CanvasGroup _canvasGroup;

        public TextMeshProUGUI Text
        {
            get
            {
                if (_text == null)
                    _text = GetComponentInChildren<TextMeshProUGUI>();
                return _text;
            }
        }

        /// <summary>
        /// This is for Text Animation Fade
        /// </summary>
        public virtual float LiveTime { get; } = 1;

        public virtual CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = GetComponentInParent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EnsureComponents()
        {
            if (_text == null) _text = Text;
            if (_canvasGroup == null) _canvasGroup = CanvasGroup;
        }

        protected virtual void Awake()
        {
            EnsureComponents();
        }

        protected virtual void OnValidate()
        {
            EnsureComponents();
        }
    }
}