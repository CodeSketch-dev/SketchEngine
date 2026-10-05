using DG.Tweening;
using UnityEngine;

namespace SketchEngine.UIView
{
    public class ViewTransitionTransformRotate : ViewTransitionTransform
    {
        // "readonly" trước đây đi cùng [SerializeField] là smell: Unity không đảm bảo set field
        // readonly nhất quán qua Inspector/deserialize trên mọi phiên bản - đã bỏ readonly.
        [SerializeField] RotateMode _rotateMode = RotateMode.FastBeyond360;

        public override string DisplayName => "Transform Rotate";

        protected override Vector3 GetCurrentValue(ViewTransitionEntity entity) => entity.TransformCached.localEulerAngles;

        protected override Tweener CreateTween(ViewTransitionEntity entity, Vector3 value, float duration)
            => entity.TransformCached.DOLocalRotate(value, duration, _rotateMode);
    }
}
