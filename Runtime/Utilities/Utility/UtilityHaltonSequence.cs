using UnityEngine;

namespace SketchEngine.Utilities
{
    /// <summary>
    /// Lấy mẫu chuỗi Halton dùng chung bằng cách cập nhật tăng dần (cơ số 2, 3, 5).
    /// Mỗi lần gọi Increment() đưa chuỗi dùng chung sang bước tiếp theo.
    /// </summary>
    public static class UtilityHaltonSequence
    {
        /// <summary>
        /// Vị trí Halton hiện tại, mỗi trục nằm trong khoảng [0..1].
        /// </summary>
        public static Vector3 CurrentPosition { get; private set; }

        static long _base2;
        static long _base3;
        static long _base5;

        // =====================================================
        // CẬP NHẬT CHUỖI
        // =====================================================

        /// <summary>
        /// Đưa chuỗi Halton sang bước tiếp theo.
        /// Có thể chọn các trục cần cập nhật tọa độ; bộ đếm của trục tắt vẫn tăng.
        /// </summary>
        public static long Increment(bool useX = true, bool useY = true, bool useZ = true)
        {
            if (useX) IncrementBase2();
            else _base2++;

            if (useY) IncrementBase3();
            else _base3++;

            if (useZ) IncrementBase5();
            else _base5++;

            return _base2;
        }

        // =====================================================
        // THUẬT TOÁN CHO TỪNG CƠ SỐ
        // =====================================================

        static void IncrementBase2()
        {
            long old = _base2++;
            long diff = _base2 ^ old;

            float s = 0.5f;

            while (diff > 0)
            {
                var vector3 = CurrentPosition;
                vector3.x = vector3.x + (((old & 1) == 1) ? -s : s);
                CurrentPosition = vector3;
                s *= 0.5f;

                diff >>= 1;
                old >>= 1;
            }
        }

        static void IncrementBase3()
        {
            const float inv = 1f / 3f;

            long bitmask = 0x3;
            long bitadd = 0x1;
            float s = inv;

            _base3++;

            while (true)
            {
                if ((_base3 & bitmask) == bitmask)
                {
                    _base3 += bitadd;
                    var vector3 = CurrentPosition;
                    vector3.y = vector3.y - 2f * s;
                    CurrentPosition = vector3;

                    bitmask <<= 2;
                    bitadd <<= 2;
                    s *= inv;
                }
                else
                {
                    var vector3 = CurrentPosition;
                    vector3.y = vector3.y + s;
                    CurrentPosition = vector3;
                    break;
                }
            }
        }

        static void IncrementBase5()
        {
            const float inv = 1f / 5f;

            long bitmask = 0x7;
            long bitadd = 0x3;
            long dmax = 0x5;
            float s = inv;

            _base5++;

            while (true)
            {
                if ((_base5 & bitmask) == dmax)
                {
                    _base5 += bitadd;
                    var vector3 = CurrentPosition;
                    vector3.z = vector3.z - 4f * s;
                    CurrentPosition = vector3;

                    bitmask <<= 3;
                    dmax <<= 3;
                    bitadd <<= 3;
                    s *= inv;
                }
                else
                {
                    var vector3 = CurrentPosition;
                    vector3.z = vector3.z + s;
                    CurrentPosition = vector3;
                    break;
                }
            }
        }

        // =====================================================
        // CÁC HÀM TIỆN ÍCH
        // =====================================================

        /// <summary>
        /// Lấy mẫu tiếp theo trong khối lập phương [0..1]³.
        /// </summary>
        public static Vector3 Next01(bool x = true, bool y = true, bool z = true)
        {
            Increment(x, y, z);
            return CurrentPosition;
        }

        /// <summary>
        /// Lấy mẫu tiếp theo với mỗi trục trong khoảng [-0.5 .. 0.5], tâm tại gốc tọa độ.
        /// </summary>
        public static Vector3 NextCentered()
        {
            Increment();
            return CurrentPosition - Vector3.one * 0.5f;
        }

        /// <summary>
        /// Lấy vị trí tiếp theo bên trong khối hộp theo tọa độ thế giới.
        /// </summary>
        public static Vector3 NextInBox(Vector3 center, Vector3 size)
        {
            Increment();
            return center + Vector3.Scale(
                CurrentPosition - Vector3.one * 0.5f,
                size
            );
        }

        /// <summary>
        /// Bỏ qua count mẫu bằng cách cập nhật chuỗi tương ứng số lần đó.
        /// </summary>
        public static void Skip(int count)
        {
            for (int i = 0; i < count; i++)
                Increment();
        }

        // =====================================================
        // ĐẶT LẠI CHUỖI
        // =====================================================

        public static void Reset()
        {
            CurrentPosition = Vector3.zero;
            _base2 = 0;
            _base3 = 0;
            _base5 = 0;
        }

        #region Ví dụ sử dụng

        // Ví dụ sử dụng
        // void Start()
        // {
        //     UtilityHaltonSequence.Reset();
        //
        //     for (int i = 0; i < 20; i++)
        //     {
        //         Vector3 pos =
        //             UtilityHaltonSequence.NextInBox(spawnCenter, spawnSize);
        //
        //         Instantiate(enemyPrefab, pos, Quaternion.identity);
        //     }
        // }

        // void Emit()
        // {
        //     Vector3 p = UtilityHaltonSequence.NextCentered() * 3f;
        //     SpawnDust(p);
        // }

        // void LateUpdate()
        // {
        //     Vector3 jitter =
        //         UtilityHaltonSequence.NextCentered() * 0.03f;
        //
        //     cam.transform.localPosition = jitter;
        // }

        // Vector3 p = UtilityHaltonSequence.Next01(x: true, y: false, z: true);
        // p.y = 0f;

        #endregion

    }
}
