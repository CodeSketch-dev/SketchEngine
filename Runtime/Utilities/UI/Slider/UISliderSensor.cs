using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace SketchEngine.Utilities.UI
{
    /// <summary>
    ///     Tiện ích điều khiển Slider:
    ///     - Có thể tween giá trị slider tới target.
    ///     - Người chơi có thể override ngay (OnValueChanged).
    ///     - Nếu override thì tự hủy tween.
    ///     - Có 2 mode:
    ///     * Silent: không gọi onValueChanged, nhưng bắn OnValueSilentChanged riêng.
    ///     * Notify: gọi onValueChanged như player kéo.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class UISliderSensor : MonoBehaviour
    {
        [SerializeField, HideInInspector] Slider _slider;

        public enum UpdateMode
        {
            Silent,
            Notify
        }

        Tween _tween;

        public bool IsAuto { get; private set; }

        public float Value => Slider.value;

        public Slider Slider
        {
            get
            {
                if (_slider == null)
                    _slider = GetComponentInChildren<Slider>();
                return _slider;
            }
        }

        /// <summary>
        ///     Event riêng cho Silent mode.
        ///     Sẽ được gọi khi giá trị thay đổi mà không kích hoạt onValueChanged của Slider.
        /// </summary>
        public event Action<float> OnValueSilentChanged;

        void Awake()
        {
            Slider.onValueChanged.AddListener(OnUserChanged);
        }

        void OnDestroy()
        {
            _tween.Stop();
            Slider.onValueChanged.RemoveListener(OnUserChanged);
        }

        void OnUserChanged(float v)
        {
            // Nếu player kéo tay => hủy auto
            if (IsAuto) CancelAuto();
        }

        /// <summary>
        ///     Tự động tween slider tới target trong duration (giây).
        ///     Mode:
        ///     - Silent: không gọi onValueChanged, nhưng bắn OnValueSilentChanged.
        ///     - Notify: gọi onValueChanged như player kéo.
        /// </summary>
        public void PlayTo(float target, float duration, UpdateMode mode = UpdateMode.Silent)
        {
            CancelAuto();
            IsAuto = true;

            if (mode == UpdateMode.Silent)
                _tween = Tween.Custom(
                    Slider.value,
                    target,
                    duration,
                    v =>
                    {
                        Slider.SetValueWithoutNotify(v);
                        OnValueSilentChanged?.Invoke(v);
                    }
                ).OnComplete(() => IsAuto = false);
            else
                _tween = Tween.Custom(
                    Slider.value,
                    target,
                    duration,
                    v => Slider.value = v
                ).OnComplete(() => IsAuto = false);
        }

        public void CancelAuto()
        {
            if (!IsAuto) return;
            IsAuto = false;
            _tween.Stop();
        }

        /// <summary>
        ///     Đặt giá trị thủ công (hủy auto trước).
        /// </summary>
        public void SetValue(float v, UpdateMode mode = UpdateMode.Silent)
        {
            CancelAuto();
            if (mode == UpdateMode.Silent)
            {
                Slider.SetValueWithoutNotify(v);
                OnValueSilentChanged?.Invoke(v);
            }
            else
            {
                Slider.value = v;
            }
        }
    }
}