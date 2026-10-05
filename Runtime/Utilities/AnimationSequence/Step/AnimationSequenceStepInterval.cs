using DG.Tweening;
using UnityEngine;

namespace SketchEngine.Utilities.Animations
{
    public class AnimationSequenceStepInterval : AnimationSequenceStep
    {
        [SerializeField]
        float _duration;

        public override string displayName => "Interval";

        public override void AddToSequence(AnimationSequence animationSequence)
        {
            animationSequence.Sequence.AppendInterval(_duration);
        }
    }
}