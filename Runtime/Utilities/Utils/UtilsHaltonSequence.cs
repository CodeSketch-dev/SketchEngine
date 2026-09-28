using UnityEngine;

namespace SketchEngine.Utilities.Utils
{
    /// <summary>
    /// Global incremental Halton sequence sampler (base 2,3,5).
    /// Each Increment() advances the global sequence.
    /// </summary>
    public static class UtilsHaltonSequence
    {
        /// <summary>
        /// Current Halton position in [0..1] range per axis.
        /// </summary>
        public static Vector3 CurrentPosition { get; private set; }

        static long _base2;
        static long _base3;
        static long _base5;

        // =====================================================
        // CORE
        // =====================================================

        /// <summary>
        /// Advance Halton sequence by one step.
        /// Optionally enable axes.
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
        // BASE IMPLEMENTATIONS
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
        // HELPERS (RẤT HAY DÙNG)
        // =====================================================

        /// <summary>
        /// Next sample in [0..1]³
        /// </summary>
        public static Vector3 Next01(bool x = true, bool y = true, bool z = true)
        {
            Increment(x, y, z);
            return CurrentPosition;
        }

        /// <summary>
        /// Next sample centered in [-0.5 .. 0.5]
        /// </summary>
        public static Vector3 NextCentered()
        {
            Increment();
            return CurrentPosition - Vector3.one * 0.5f;
        }

        /// <summary>
        /// Next position inside a world-space box.
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
        /// Skip N samples.
        /// </summary>
        public static void Skip(int count)
        {
            for (int i = 0; i < count; i++)
                Increment();
        }

        // =====================================================
        // RESET
        // =====================================================

        public static void Reset()
        {
            CurrentPosition = Vector3.zero;
            _base2 = 0;
            _base3 = 0;
            _base5 = 0;
        }

        #region Example

        // Example
        // void Start()
        // {
        //     UtilsHaltonSequence.Reset();
        //
        //     for (int i = 0; i < 20; i++)
        //     {
        //         Vector3 pos =
        //             UtilsHaltonSequence.NextInBox(spawnCenter, spawnSize);
        //
        //         Instantiate(enemyPrefab, pos, Quaternion.identity);
        //     }
        // }
        
        // void Emit()
        // {
        //     Vector3 p = UtilsHaltonSequence.NextCentered() * 3f;
        //     SpawnDust(p);
        // }
        
        // void LateUpdate()
        // {
        //     Vector3 jitter =
        //         UtilsHaltonSequence.NextCentered() * 0.03f;
        //
        //     cam.transform.localPosition = jitter;
        // }

        // Vector3 p = UtilsHaltonSequence.Next01(x: true, y: false, z: true);
        // p.y = 0f;

        #endregion

    }
}
