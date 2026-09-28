using System;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

using SketchEngine.Core.UI;

namespace SketchEngine.UIPopup
{
    public class PopupButtonNoThanks : UIButtonBase
    {
        [SerializeField] Popup _popup;
        
        [Title("Config")]
        [SerializeField] float _delay = 0.5f;
        [SerializeField] float _scaleDuration = 0.25f;
        [SerializeField] Ease _ease = Ease.Linear;
        [SerializeField] UnityEvent _eventOnClick;
        
        public event Action ActionOnClick;

        Tween _tweenDelay;
        Tween _tweenScale;
        
        protected override void Awake()
        {
            base.Awake();

            EnsureComponents();
                
            TransformCached.localScale = Vector3.zero;
            _tweenDelay = DOVirtual.DelayedCall(_popup.OpenDuration + _delay, Show, false);
        }

        void OnDestroy()
        {
            _tweenDelay?.Kill();
            _tweenScale?.Kill();
        }

        void Show()
        {
            _tweenScale = TransformCached.DOScale(Vector3.one, _scaleDuration).SetEase(_ease);
        }

        public override void Button_OnClick()
        {
            base.Button_OnClick();
            
            Button.interactable = false;
            
            _eventOnClick?.Invoke();
            ActionOnClick?.Invoke();
            
            _popup.Close();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void EnsureComponents()
        {
            if (_popup == null)
                _popup = GetComponentInParent<Popup>();
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            EnsureComponents();
        }
    }  
}
