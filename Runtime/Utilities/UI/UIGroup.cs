using System;
using SketchEngine.Mono;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace SketchEngine.Utilities.UI
{
    /// <summary>
    /// Quản lý hiển thị và ẩn một UI Group có hiệu ứng fade (CanvasGroup).
    /// Gồm sự kiện callback, UnityEvent, và DOTween hỗ trợ fade.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIGroup : MonoCached
    {
        [Title("Config")]
        [SerializeField] float _fadeInDuration = 0.3f;
        [SerializeField] Ease _fadeInEase = Ease.OutSine;
        [SerializeField] float _fadeOutDuration = 0.2f;
        [SerializeField] Ease _fadeOutEase = Ease.Linear;

        [SerializeField] UnityEvent _eventShow;
        [SerializeField] UnityEvent _eventHide;

        CanvasGroup _canvasGroup;
        Tween _tween;

        public Action OnShow;
        public Action OnShowComplete;
        public Action OnHide;
        public Action OnHideComplete;

        /// <summary>CanvasGroup được cache</summary>
        public CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();

        void OnDestroy()
        {
            _tween.Stop();
        }

        /// <summary>
        /// Hiện UI Group với hiệu ứng fade.
        /// </summary>
        public void Show()
        {
            GameObjectCached.SetActive(true);

            var cg = CanvasGroup;
            cg.blocksRaycasts = true;
            cg.interactable = true;

            float duration = _fadeInDuration * (1.0f - cg.alpha);

            OnShow?.Invoke();
            _eventShow?.Invoke();

            _tween.Stop();
            _tween = Tween.Alpha(cg, 1f, duration, ease: _fadeInEase).OnComplete(HandleShowComplete);
        }

        /// <summary>
        /// Ẩn UI Group với hiệu ứng fade hoặc ngay lập tức.
        /// </summary>
        /// <param name="immediately">Nếu true thì ẩn ngay lập tức không tween</param>
        public void Hide(bool immediately = false)
        {
            if (!GameObjectCached.activeInHierarchy)
                return;

            var cg = CanvasGroup;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            OnHide?.Invoke();
            _eventHide?.Invoke();

            _tween.Stop();

            if (immediately)
            {
                OnHideComplete?.Invoke();
                GameObjectCached.SetActive(false);
                return;
            }

            _tween = Tween.Alpha(cg, 1f, _fadeOutDuration, ease: _fadeOutEase).OnComplete(HandleHideComplete);
        }

        /// <summary>
        /// Gọi sau khi show xong.
        /// </summary>
        void HandleShowComplete()
        {
            OnShowComplete?.Invoke();
        }

        /// <summary>
        /// Gọi sau khi hide xong.
        /// </summary>
        void HandleHideComplete()
        {
            OnHideComplete?.Invoke();
            GameObjectCached.SetActive(false);
        }
    }
}
