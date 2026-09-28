using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;

using SketchEngine.Core;
using SketchEngine.Core.Extensions;
using SketchEngine.Mono;
using SketchEngine.UIPopup;
using PrimeTween;

namespace SketchEngine.Notifications
{
    public class UINotificationText : MonoCached
    {
        static readonly UINotificationText[] _pool = new UINotificationText[1];
        static int _poolIndex = 0;
        const float DesignCanvasWidth = 1280f;

        [Header("Reference")]
        [SerializeField] TextMeshProUGUI _txtMain;

        [Header("Config")]
        [SerializeField] float _fadeInDuration = 0.35f;
        [SerializeField] float _scaleDuration = 0.25f;
        [SerializeField] Ease _scaleEase = Ease.Linear;
        [MinMaxSlider(0f, 1f, ShowFields = true)]
        [SerializeField] Vector2 _anchorY = new Vector2(0.7f, 0.75f);
        [SerializeField] float _moveDuration = 0.4f;
        [SerializeField] Ease _moveEase = Ease.Linear;
        [SerializeField] float _fadeOutDelay = 0.5f;
        [SerializeField] float _fadeOutDuration = 0.9f;

        Sequence _sequence;
        RectTransform _target;
        CanvasGroup _canvasGroup;
        RectTransform _parentRect;

        void Awake()
        {
            _target = TransformCached.GetChild(0).GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _parentRect = TransformCached.GetComponent<RectTransform>();
        }

        void OnDestroy()
        {
            _sequence.Stop();
        }

        public void Show(string msg, float displayDuration = -1f)
        {
            _txtMain.text = msg;

            InitSequence(displayDuration >= 0f ? displayDuration : _fadeOutDelay);
        }

        void InitSequence(float displayDuration)
        {
            _sequence.Stop(); // Stop any existing sequence

            float rectHeight = _parentRect.rect.height;
            float scaleFactor = 1f;
            Canvas popupCanvas = PopupManager.Canvas;
            if (popupCanvas != null && popupCanvas.pixelRect.width > 0f)
                scaleFactor = popupCanvas.pixelRect.width / DesignCanvasWidth;
            float scaleYOffset = (_target.rect.height * (scaleFactor - 1f)) * 0.5f;

            // Reset position, scale, alpha
            Vector2 startPos = _target.anchoredPosition;
            startPos.y = rectHeight * _anchorY.x - rectHeight * 0.5f + scaleYOffset;
            _target.anchoredPosition = startPos;
            _target.localScale = Vector3.zero;
            _canvasGroup.alpha = 0f;

            float endY = rectHeight * _anchorY.y - rectHeight * 0.5f + scaleYOffset;

            _sequence = Sequence.Create()
                .Group(Tween.Scale(_target, scaleFactor, _scaleDuration, _scaleEase))
                .Group(Tween.Alpha(_canvasGroup, 0f, 1f, _fadeInDuration))
                .Group(Tween.UIAnchoredPositionY(_target, startPos.y, endY, _moveDuration, _moveEase))
                .ChainDelay(displayDuration)
                .Chain(Tween.Alpha(_canvasGroup, 1f, 0f, _fadeOutDuration))
                .OnComplete(OnComplete, false);
        }

        void OnComplete()
        {
            GameObjectCached.SetActive(false);
        }

        public static void Push(string msg, float displayDuration = -1f)
        {
            if (_poolIndex >= _pool.Length)
                _poolIndex = 0;

            if (_pool[_poolIndex] == null)
            {
                _pool[_poolIndex] = CodeSketchFactory.UINotificationText.Create().GetComponent<UINotificationText>();
            }

            _pool[_poolIndex].TransformCached.SetParent(PopupManager.Root, false);
            _pool[_poolIndex].TransformCached.SetAsLastSibling();
            _pool[_poolIndex].GameObjectCached.SetActive(true);
            _pool[_poolIndex].Show(msg, displayDuration);

            _poolIndex++;
        }
    }
}
