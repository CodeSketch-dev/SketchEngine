using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    public static class ExtensionsVector3
    {
        /// <summary>Lấy Vector3 chỉ chứa trục X và Y (z = 0).</summary>
        public static Vector3 XY(this Vector3 v3) => new Vector3(v3.x, v3.y, 0);

        /// <summary>Lấy Vector3 chỉ chứa trục X và Z (y = 0).</summary>
        public static Vector3 XZ(this Vector3 v3) => new Vector3(v3.x, 0, v3.z);

        /// <summary>Lấy Vector3 chỉ chứa trục Y và Z (x = 0).</summary>
        public static Vector3 YZ(this Vector3 v3) => new Vector3(0, v3.y, v3.z);

        /// <summary>Lấy Vector3 chỉ có trục X, còn lại = 0.</summary>
        public static Vector3 X(this Vector3 v3) => new Vector3(v3.x, 0, 0);

        /// <summary>Lấy Vector3 chỉ có trục Y, còn lại = 0.</summary>
        public static Vector3 Y(this Vector3 v3) => new Vector3(0, v3.y, 0);

        /// <summary>Lấy Vector3 chỉ có trục Z, còn lại = 0.</summary>
        public static Vector3 Z(this Vector3 v3) => new Vector3(0, 0, v3.z);

        /// <summary>Trả về độ dài của vector chỉ tính X và Z.</summary>
        public static float XZMagnitude(this Vector3 v) => v.XZ().magnitude;

        /// <summary>Nhân từng trục của vector b với vector a.</summary>
        public static Vector3 MultipliedBy(this in Vector3 a, Vector3 b)
        {
            b.x *= a.x;
            b.y *= a.y;
            b.z *= a.z;
            return b;
        }

        /// <summary>Làm tròn Vector3 sang Vector3Int.</summary>
        public static Vector3Int RoundToInt(this Vector3 vector) =>
            new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z));

        /// <summary>Làm tròn xuống Vector3 sang Vector3Int.</summary>
        public static Vector3Int FloorToInt(this Vector3 vector) =>
            new Vector3Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z));

        /// <summary>Làm tròn lên Vector3 sang Vector3Int.</summary>
        public static Vector3Int CeilToInt(this Vector3 vector) =>
            new Vector3Int(Mathf.CeilToInt(vector.x), Mathf.CeilToInt(vector.y), Mathf.CeilToInt(vector.z));

        /// <summary>Tính tỉ lệ nội suy ngược giữa hai vector.</summary>
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
        }

        /// <summary>Chiếu vector lên 1 vector đường thẳng.</summary>
        public static Vector3 ProjectOnLine(this Vector3 vectorToProject, Vector3 line)
        {
            float dot = Vector3.Dot(vectorToProject, line);
            float sqrMag = line.sqrMagnitude;
            return (dot / sqrMag) * line;
        }

        /// <summary>Tạo Quaternion từ direction (LookRotation). Trả về Quaternion.identity nếu direction = Vector3.zero.</summary>
        public static Quaternion QuaternionLookDirection(this Vector3 direction)
        {
            return direction == Vector3.zero ? UnityEngine.Quaternion.identity : UnityEngine.Quaternion.LookRotation(direction);
        }

        /// <summary>Chuyển Euler angles sang Quaternion.</summary>
        public static Quaternion Quaternion(this Vector3 eulers)
        {
            return UnityEngine.Quaternion.Euler(eulers);
        }

        /// <summary>Chuyển direction sang EulerAngles. Nếu direction = Vector3.zero, trả về Vector3.zero.</summary>
        public static Vector3 EulerAngles(this Vector3 direction)
        {
            if (direction == Vector3.zero)
                return Vector3.zero;

            return UnityEngine.Quaternion.LookRotation(direction).eulerAngles;
        }

        /// <summary>Chuyển Euler angles sang vector direction (forward).</summary>
        public static Vector3 Direction(this Vector3 eulerAngles)
        {
            return UnityEngine.Quaternion.Euler(eulerAngles) * Vector3.forward;
        }

        /// <summary>Giới hạn vector thành giá trị trong min và max cho từng trục.</summary>
        public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(v.x, min.x, max.x),
                Mathf.Clamp(v.y, min.y, max.y),
                Mathf.Clamp(v.z, min.z, max.z));
        }

        /// <summary>Kiểm tra xem vector có phải là NaN hoặc vô cực không.</summary>
        public static bool IsInvalid(this Vector3 v)
        {
            return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z)
                || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z);
        }

        /// <summary>Trả về một vector ngẫu nhiên nằm trong khoảng từ -v đến +v.</summary>
        public static Vector3 RandomRangeAround(this Vector3 v)
        {
            return new Vector3(
                Random.Range(-v.x, v.x),
                Random.Range(-v.y, v.y),
                Random.Range(-v.z, v.z));
        }
        
        // So sánh gần đúng giữa 2 Vector2
        public static bool Approximately(this Vector3 a, Vector3 b, float tolerance)
        {
            return (a - b).sqrMagnitude <= tolerance * tolerance;
        }
    }
}
