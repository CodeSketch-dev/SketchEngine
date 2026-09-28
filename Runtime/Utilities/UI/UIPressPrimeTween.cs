using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PrimeTween;

namespace SketchEngine.Utilities.UI
{
    public class UIPressPrimeTween : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Reference")]
        [SerializeField] Transform _target;

        [Header("Config")]
        [SerializeField] float _scaleThreshold = 0.9f; // ví dụ: 0.9 = 90%
        [Min(0.01f)]
        [SerializeField] float _duration = 0.1f;

        Vector3 _startScale;
        
        Tween _tween;
        bool _isDown;

        Button _button;
        Button button => _button != null ? _button : (_button = GetComponent<Button>());

        void Awake()
        {
            if (_target == null)
                _target = transform;

            _startScale = _target.localScale;
        }

        void OnEnable()
        {
            _target.localScale = _startScale;
        }

        void OnDisable()
        {
            _tween.Stop();
            _target.localScale = _startScale;
            _isDown = false;
        }

        void OnDestroy()
        {
            _tween.Stop();
        }

        void ScaleTo(float percent)
        {
            Vector3 targetScale = _startScale * percent;
            _tween.Stop();
            _tween = Tween.Scale(_target, _target.localScale, targetScale, _duration, Ease.OutQuad, useUnscaledTime: true);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;

            _isDown = true;
            ScaleTo(_scaleThreshold);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!_isDown) return;

            _isDown = false;
            ScaleTo(1f);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (_isDown)
            {
                _isDown = false;
                ScaleTo(1f);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (_isDown)
            {
                ScaleTo(_scaleThreshold);
            }
        }
    }
}
