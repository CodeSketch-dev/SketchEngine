using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Utilities
{
    /// <summary>
    /// Thành phần tạo chuyển động dao động.
    ///
    /// Mục đích:
    /// - Tạo chuyển động liên tục theo sóng sin cho GameObject
    /// - Phù hợp cho bệ nổi, vật phẩm có thể thu thập, vật trang trí hoặc hiệu ứng chuyển động đơn giản
    ///
    /// Cách hoạt động:
    /// - Đối tượng dao động quanh vị trí ban đầu
    /// - Chuyển động theo sóng sin: sin(2πt / period)
    /// - Có thể tùy chỉnh hướng, biên độ và chu kỳ
    ///
    /// Lưu ý:
    /// - Cần gọi Tick() mỗi khung hình để cập nhật chuyển động; không cập nhật theo hệ thống vật lý
    /// - Gán trực tiếp vị trí của Transform (không đảm bảo tương tác vật lý đúng)
    /// - Phù hợp nhất cho các đối tượng hiển thị hoặc không sử dụng vật lý
    /// </summary>
    public class Oscillator : MonoCached
    {
        /// <summary>
        /// Khoảng cách tối đa tính từ vị trí ban đầu.
        /// </summary>
        [Tooltip("Biên độ dao động: khoảng cách tối đa từ tâm dao động khi hướng đã được chuẩn hóa.")]
        [SerializeField] float _amplitude = 1.0f;

        /// <summary>
        /// Thời gian (tính bằng giây) để hoàn thành một chu kỳ dao động.
        /// </summary>
        [Tooltip("Chu kỳ dao động: thời gian tính bằng giây để hoàn thành một lần dao động.")]
        [SerializeField] float _period = 1.0f;

        /// <summary>
        /// Hướng dao động.
        /// Ví dụ: Vector3.up, Vector3.right hoặc một vectơ đã được chuẩn hóa bất kỳ.
        /// </summary>
        [Tooltip("Hướng dao động. Dùng vectơ có độ dài bằng 1 để giữ đúng biên độ đã đặt.")]
        [SerializeField] Vector3 _direction = Vector3.up;

        /// <summary>
        /// Vị trí ban đầu được lưu lại để làm tâm dao động.
        /// </summary>
        Vector3 _startPosition;

        // =====================================================
        // VÒNG ĐỜI
        // =====================================================

        /// <summary>
        /// Được gọi một lần khi đối tượng được khởi tạo.
        /// Lưu lại vị trí ban đầu để thực hiện dao động.
        /// </summary>
        protected virtual void Start()
        {
            _startPosition = TransformCached.position;
        }

        /// <summary>
        /// Cập nhật chuyển động mỗi khi Tick() được gọi.
        /// Cộng độ lệch theo sóng sin vào vị trí ban đầu đã lưu.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            // Bỏ qua cập nhật nếu chu kỳ không hợp lệ
            if (_period <= 0.0001f)
                return;

            float phase = Mathf.Sin(2.0f * Mathf.PI * Time.time / _period);
            Vector3 offset = _direction * _amplitude * phase;
            TransformCached.position = _startPosition + offset;
        }
    }
}
