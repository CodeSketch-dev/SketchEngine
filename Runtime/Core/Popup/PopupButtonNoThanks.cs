using System;
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

            float openDuration = _popup ? _popup.OpenDuration : 0f;
            _tweenDelay = DOVirtual.DelayedCall(openDuration + _delay, Show, false);
        }

        void OnDestroy()
        {
            _tweenDelay?.Kill();
            _tweenScale?.Kill();
        }

        void Show()
        {
            _tweenScale?.Kill();
            _tweenScale = TransformCached.DOScale(Vector3.one, _scaleDuration).SetEase(_ease);
        }

        public override void Button_OnClick()
        {
            base.Button_OnClick();

            if (!_popup) return;

            Button.interactable = false;

            _eventOnClick?.Invoke();
            ActionOnClick?.Invoke();

            if (!_popup) return;

            _popup.Close();

            // Close có thể bị bỏ qua khi popup còn đang mở animation; khi đó trả lại khả năng bấm để không khóa nút vĩnh viễn.
            if (!_popup.IsClosing && Button) Button.interactable = true;
        }

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
