using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SketchEngine.UIView
{
    /// <summary>
    /// Base chung cho các transition dạng Vector3 trên Transform (move/rotate/scale). Các class con
    /// trước đây tự lặp lại y hệt logic "lấy giá trị hiện tại nếu keepEnd/keepStart" - gộp về đây
    /// theo template method để sửa 1 chỗ áp dụng cho cả 3, tránh sửa 1 nơi quên nơi khác.
    /// </summary>
    public abstract class ViewTransitionTransform : ViewTransition
    {
        [SerializeField] protected Ease _ease = Ease.Linear;

        [SerializeField] protected bool _keepEnd = true;

        [HideIf("@_keepEnd")]
        [SerializeField] protected Vector3 _valueEnd;

        [SerializeField] protected bool _keepStart = false;

        [HideIf("@_keepStart")]
        [SerializeField] protected Vector3 _valueStart;

        /// <summary>Đọc giá trị Vector3 hiện tại của transform (vị trí/góc xoay/tỉ lệ tùy class con).</summary>
        protected abstract Vector3 GetCurrentValue(ViewTransitionEntity entity);

        /// <summary>
        /// Tạo tween DOTween tương ứng (DOAnchorPos3D/DOLocalRotate/DOScale...) với giá trị đích đã tính sẵn.
        /// Phải trả về <see cref="Tweener"/> (không phải Tween) vì ChangeStartValue chỉ là extension method
        /// của Tweener, dùng kiểu Tween cơ sở sẽ không resolve được.
        /// </summary>
        protected abstract Tweener CreateTween(ViewTransitionEntity entity, Vector3 value, float duration);

        public sealed override Tween GetTween(ViewTransitionEntity entity, float duration)
        {
            Vector3 value = _keepEnd ? GetCurrentValue(entity) : _valueEnd;
            Vector3 valueStart = _keepStart ? GetCurrentValue(entity) : _valueStart;

            return CreateTween(entity, value, duration)
                .ChangeStartValue(valueStart)
                .SetEase(_ease);
        }
    }
}
