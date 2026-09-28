using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.Utilities.Animations
{
    public abstract class AnimationSequenceStepTransform : AnimationSequenceStepAction<Transform>
    {
        [SerializeField]
        protected Vector3 _value;

        [SerializeField]
        [ShowIf("@_changeStartValue")]
        protected Vector3 _valueStart;
    }
}