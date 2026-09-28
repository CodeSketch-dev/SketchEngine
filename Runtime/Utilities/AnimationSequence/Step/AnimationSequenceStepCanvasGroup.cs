using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.Utilities.Animations
{
    public class AnimationSequenceStepCanvasGroup : AnimationSequenceStepAction<CanvasGroup>
    {
        [SerializeField]
        [Range(0f, 1f)]
        readonly float _alpha = 1.0f;

        [SerializeField]
        [Range(0f, 1f), ShowIf("@_changeStartValue")]
        readonly float _alphaStart = 0.0f;

        public override string displayName => $"{(_isSelf ? "CanvasGroup (This)" : _owner)}: DOFade";

        protected override Tween GetTween(AnimationSequence animationSequence)
        {
            CanvasGroup owner = _isSelf ? animationSequence.GetComponent<CanvasGroup>() : _owner;

            float duration = _isSpeedBased ? Mathf.Abs(_alpha - owner.alpha) / _duration : _duration;
            float start = _changeStartValue ? _alphaStart : owner.alpha;
            float end = _relative ? owner.alpha + _alpha : _alpha;

            Tween tween = owner.DOFade(end, duration)
                               .ChangeStartValue(start);

            owner.alpha = end;

            return tween;
        }

        protected override Tween GetResetTween(AnimationSequence animationSequence)
        {
            CanvasGroup owner = _isSelf ? animationSequence.GetComponent<CanvasGroup>() : _owner;

            return owner.DOFade(owner.alpha, 0.0f);
        }
    }
}