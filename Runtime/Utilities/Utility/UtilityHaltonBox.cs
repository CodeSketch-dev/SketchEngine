using UnityEngine;

namespace SketchEngine.Utilities
{
    /// <summary>
    /// Lấy mẫu vị trí trong khối hộp bằng chuỗi Halton dùng chung.
    /// Chỉ số static giúp các hệ thống cùng sử dụng một chuỗi lấy mẫu.
    /// </summary>
    public static class UtilityHaltonBox
    {
        static int _index;

        // =====================================================
        // ĐIỀU KHIỂN
        // =====================================================

        /// <summary>
        /// Đặt lại chỉ số Halton dùng chung; giá trị âm được đưa về 0.
        /// </summary>
        public static void Reset(int startIndex = 0)
        {
            _index = Mathf.Max(0, startIndex);
        }

        /// <summary>
        /// Chỉ số dùng cho lần lấy mẫu tiếp theo (chỉ đọc).
        /// </summary>
        public static int CurrentIndex => _index;

        // =====================================================
        // LẤY MẪU VỊ TRÍ
        // =====================================================

        /// <summary>
        /// Lấy vị trí tiếp theo trong khối hộp; chuỗi Halton giúp các điểm phân bố tương đối đều.
        /// </summary>
        public static Vector3 NextPosition(Vector3 center, Vector3 size)
        {
            float x = Halton(_index, 2);
            float y = Halton(_index, 3);
            float z = Halton(_index, 5);
            _index++;

            return center + new Vector3(
                (x - 0.5f) * size.x,
                (y - 0.5f) * size.y,
                (z - 0.5f) * size.z
            );
        }

        /// <summary>
        /// Lấy vị trí tiếp theo trong khối hộp có góc xoay.
        /// </summary>
        public static Vector3 NextPosition(
            Vector3 center,
            Vector3 size,
            Quaternion rotation)
        {
            Vector3 local = NextPosition(Vector3.zero, size);
            return center + rotation * local;
        }

        /// <summary>
        /// Lấy mẫu trên mặt phẳng XZ; y là độ lệch theo trục Y so với center.
        /// </summary>
        public static Vector3 NextPosition2D(
            Vector3 center,
            Vector2 sizeXZ,
            float y = 0f)
        {
            float x = Halton(_index, 2);
            float z = Halton(_index, 3);
            _index++;

            return center + new Vector3(
                (x - 0.5f) * sizeXZ.x,
                y,
                (z - 0.5f) * sizeXZ.y
            );
        }

        // =====================================================
        // LẤY MẪU HÀNG LOẠT
        // =====================================================

        /// <summary>
        /// Điền các vị trí phân bố theo chuỗi Halton vào mảng buffer đã cấp phát.
        /// </summary>
        public static void FillBatch(
            Vector3 center,
            Vector3 size,
            Vector3[] buffer)
        {
            if (buffer == null) return;

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = NextPosition(center, size);
        }

        // =====================================================
        // TÍNH GIÁ TRỊ HALTON
        // =====================================================

        static float Halton(int index, int b)
        {
            float result = 0f;
            float f = 1f / b;

            while (index > 0)
            {
                result += f * (index % b);
                index /= b;
                f /= b;
            }

            return result;
        }

        // Ví dụ sử dụng: nhấn phím Space để tạo đối tượng tại vị trí lấy mẫu
        // void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         Vector3 pos = UtilityHaltonBox.NextPosition(spawnCenter, spawnSize);
        //         Instantiate(prefab, pos, Quaternion.identity);
        //     }
        // }
    }
}
