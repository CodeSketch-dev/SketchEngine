using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace SketchEngine.Utilities.Animations
{
    public class AnimationSequenceStepCallback : AnimationSequenceStep
    {
        [SerializeField]
        bool _isInserted;

        [SerializeField]
        [Min(0f), ShowIf("@_isInserted")]
        float _insertTime;

        [SerializeField]
        UnityEvent _callback;

        public override string displayName => "Callback";

        public override void AddToSequence(AnimationSequence animationSequence)
        {
            if (_isInserted)
                animationSequence.Sequence.InsertCallback(_insertTime, () => { _callback?.Invoke(); });
            else
                animationSequence.Sequence.AppendCallback(() => { _callback?.Invoke(); });
        }
    }
}