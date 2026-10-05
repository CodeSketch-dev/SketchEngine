using DG.Tweening;
using System;

namespace SketchEngine.UIView
{
    [Serializable]
    public abstract class ViewExtra
    {
        public virtual string DisplayName { get; }

        public void Apply(View view)
        {
            Tween tween = GetTween(view, 1.0f);

            view.sequence.Join(tween);
        }

        protected virtual Tween GetTween(View view, float duration)
        {
            return null;
        }
    }
}
