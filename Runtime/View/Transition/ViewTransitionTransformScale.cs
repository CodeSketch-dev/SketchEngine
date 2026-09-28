using DG.Tweening;
using UnityEngine;

namespace SketchEngine.UIView
{
    public class ViewTransitionTransformScale : ViewTransitionTransform
    {
        public override string DisplayName => "RectTransform Scale";

        public override Tween GetTween(ViewTransitionEntity entity, float duration)
        {
            Vector3 value = _keepEnd ? entity.TransformCached.localScale : _valueEnd;
            Vector3 valueStart = _keepStart ? entity.TransformCached.localScale : _valueStart;

            return entity.TransformCached.DOScale(value, duration)
                                         .ChangeStartValue(valueStart)
                                         .SetEase(_ease);
        }
    }
}