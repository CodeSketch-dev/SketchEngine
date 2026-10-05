using DG.Tweening;
using UnityEngine;

namespace SketchEngine.UIView
{
    public class ViewTransitionTransformMove : ViewTransitionTransform
    {
        public override string DisplayName => "Transform Move";

        protected override Vector3 GetCurrentValue(ViewTransitionEntity entity) => entity.RectTransformCached.anchoredPosition3D;

        protected override Tweener CreateTween(ViewTransitionEntity entity, Vector3 value, float duration)
            => entity.RectTransformCached.DOAnchorPos3D(value, duration);
    }
}
