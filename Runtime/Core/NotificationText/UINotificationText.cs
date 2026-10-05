using System;
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
        const float DesignCanvasWidth = 1920f;

        static UINotificationText _current;

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
        Action _onSequenceComplete;

        void Awake()
        {
            _target = TransformCached.GetChild(0).GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _parentRect = TransformCached.GetComponent<RectTransform>();
            _onSequenceComplete = OnSequenceComplete;
        }

        void OnDestroy()
        {
            _sequence.Stop();

            if (_current == this)
                _current = null;
        }

        public void Show(string msg, float displayDuration = -1f)
        {
            if (_txtMain.text != msg)
                _txtMain.text = msg;

            InitSequence(displayDuration >= 0f ? displayDuration : _fadeOutDelay);
        }

        void InitSequence(float displayDuration)
        {
            _sequence.Stop();

            float rectHeight = _parentRect.rect.height;
            float scaleFactor = 1f;
            Canvas popupCanvas = PopupManager.Canvas;
            if (popupCanvas != null && popupCanvas.pixelRect.width > 0f)
                scaleFactor = popupCanvas.pixelRect.width / DesignCanvasWidth;
            float scaleYOffset = (_target.rect.height * (scaleFactor - 1f)) * 0.5f;

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
                .OnComplete(_onSequenceComplete, false);
        }

        void OnSequenceComplete()
        {
            GameObjectCached.SetActive(false);
        }

        public static void Push(string msg, float displayDuration = -1f)
        {
            if (_current == null)
            {
                GameObject prefab = SketchEngineFactory.UINotificationText.Create();
                if (prefab == null) return;

                _current = prefab.GetComponent<UINotificationText>();
                if (_current == null) return;
            }

            _current.TransformCached.SetParent(PopupManager.Root, false);
            _current.TransformCached.SetAsLastSibling();
            _current.GameObjectCached.SetActive(true);
            _current.Show(msg, displayDuration);
        }
    }
}
