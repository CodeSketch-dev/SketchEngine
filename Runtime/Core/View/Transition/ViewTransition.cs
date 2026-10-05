using DG.Tweening;
using System;

namespace SketchEngine.UIView
{
    [Serializable]
    public abstract class ViewTransition
    {
        public virtual string DisplayName { get; }

        public virtual Tween GetTween(ViewTransitionEntity entity, float duration)
        {
            return null;
        }
    }
}
