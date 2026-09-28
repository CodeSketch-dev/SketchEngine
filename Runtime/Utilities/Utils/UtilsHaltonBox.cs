using UnityEngine;

namespace SketchEngine.Utilities.Utils
{
    /// <summary>
    /// Global Halton sequence sampler for Box volume.
    /// Static index → shared sequence across systems.
    /// </summary>
    public static class UtilsHaltonBox
    {
        static int _index;

        // =====================================================
        // CONTROL
        // =====================================================

        /// <summary>
        /// Reset global Halton index.
        /// </summary>
        public static void Reset(int startIndex = 0)
        {
            _index = Mathf.Max(0, startIndex);
        }

        /// <summary>
        /// Current global index (read-only).
        /// </summary>
        public static int CurrentIndex => _index;

        // =====================================================
        // CORE
        // =====================================================

        /// <summary>
        /// Get next evenly-distributed position inside a box.
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
        /// Same as NextPosition but supports rotated box.
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
        /// 2D version (XZ plane by default).
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
        // BATCH
        // =====================================================

        /// <summary>
        /// Fill buffer with Halton-distributed positions.
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
        // HALTON CORE
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
        
        // Example
        // void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         Vector3 pos = UtilsHaltonBox.NextPosition(spawnCenter, spawnSize);
        //         Instantiate(prefab, pos, Quaternion.identity);
        //     }
        // }
    }
}
