using SketchEngine.Core.Text;
using DG.Tweening;
using UnityEngine;

namespace SketchEngine.Utilities.Text
{
    [System.Serializable]
    public class TextWorldCanvasAnimationFade : TextWorldCanvasAnimation
    {
        public override string DisplayName => "Fade";
        
        [SerializeField] float _fadeStart = 0f;
        [SerializeField] float _fadeEnd = 1f;
        [SerializeField] AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
        
        protected override Tween GetTween(UITextWorldCanvas textBase, float duration)
        {
            return textBase.CanvasGroup.DOFade(_fadeEnd, duration)
                .SetEase(_curve)
                .ChangeStartValue(_fadeStart);
        }

        protected override PrimeTween.Tween GetPrimeTween(UITextWorldCanvas textBase, float duration)
        {
            var canvasGroup = textBase.CanvasGroup;

            return PrimeTween.Tween.Alpha(
                canvasGroup,
                _fadeStart,
                _fadeEnd,
                duration,
                PrimeTween.Easing.Curve(_curve)
            );
        }
    }
}