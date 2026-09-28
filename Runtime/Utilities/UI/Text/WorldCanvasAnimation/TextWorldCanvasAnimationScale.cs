using SketchEngine.Core.Text;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.Utilities.Text
{
    [System.Serializable]
    public class TextWorldCanvasAnimationScale : TextWorldCanvasAnimation
    {
        [SerializeField] Transform _target;
        [SerializeField] float _scaleStart = 0.3f;
        [SerializeField] float _scaleEnd = 1f;
        [SerializeField] bool _relative;

        [SerializeField] bool _useCurve;
        [ShowIf("@_useCurve == false")]
        [SerializeField] Ease _ease = Ease.OutBack;
        [ShowIf("@_useCurve == true")]
        [SerializeField] AnimationCurve _curve;

        Transform _trans;
        
        public override string DisplayName => "Scale";

        // DOTween version
        protected override Tween GetTween(UITextWorldCanvas textBase, float duration)
        {
            _trans = _target == null ? textBase.transform : _target;

            float scaleStart = _scaleStart;
            float scaleEnd = _scaleEnd;
            if (_relative)
            {
                scaleStart *= _trans.localScale.x;
                scaleEnd *= _trans.localScale.x;
            }

            Tween tween = null;
            if (_useCurve)
            {
                tween = _trans.DOScale(scaleEnd, duration).SetEase(_curve)
                    .ChangeStartValue(Vector3.one * scaleStart);
            }
            else
            {
                tween = _trans.DOScale(scaleEnd, duration).SetEase(_ease)
                    .ChangeStartValue(Vector3.one * scaleStart);
            }

            return tween;
        }

        // PrimeTween version
        protected override PrimeTween.Tween GetPrimeTween(UITextWorldCanvas textBase, float duration)
        {
            _trans = _target == null ? textBase.transform : _target;

            float scaleStart = _scaleStart;
            float scaleEnd = _scaleEnd;
            if (_relative)
            {
                scaleStart *= _trans.localScale.x;
                scaleEnd *= _trans.localScale.x;
            }

            var ease = _useCurve ? PrimeTween.Easing.Curve(_curve) : EaseMapper.MapEase(_ease);
            return PrimeTween.Tween.Scale(
                _trans,
                Vector3.one * scaleStart,
                Vector3.one * scaleEnd,
                duration,
                ease
            );
        }
    }
}
