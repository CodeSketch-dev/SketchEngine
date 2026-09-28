using DG.Tweening;
using UnityEngine;

namespace SketchEngine.UIView
{
    public class ViewTransitionTransformMove : ViewTransitionTransform
    {
        public override string DisplayName => "Transform Move";

        public override Tween GetTween(ViewTransitionEntity entity, float duration)
        {
            Vector3 value = _keepEnd ? entity.rectTransform.anchoredPosition3D : _valueEnd;
            Vector3 valueStart = _keepStart ? entity.rectTransform.anchoredPosition3D : _valueStart;

            return entity.rectTransform.DOAnchorPos3D(value, duration)
                                       .ChangeStartValue(valueStart)
                                       .SetEase(_ease);
        }
    }
}
