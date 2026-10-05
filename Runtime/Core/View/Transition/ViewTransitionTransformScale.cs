using DG.Tweening;
using UnityEngine;

namespace SketchEngine.UIView
{
    public class ViewTransitionTransformScale : ViewTransitionTransform
    {
        public override string DisplayName => "RectTransform Scale";

        protected override Vector3 GetCurrentValue(ViewTransitionEntity entity) => entity.TransformCached.localScale;

        protected override Tweener CreateTween(ViewTransitionEntity entity, Vector3 value, float duration)
            => entity.TransformCached.DOScale(value, duration);
    }
}