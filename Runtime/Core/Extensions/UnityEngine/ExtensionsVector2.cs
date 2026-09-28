using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    public static class ExtensionsVector2
    {
        /// <summary>
        /// Nhân từng thành phần của vector b với a.
        /// </summary>
        public static Vector2 MultipliedBy(this in Vector2 a, Vector2 b)
        {
            b.x *= a.x;
            b.y *= a.y;
            return b;
        }

        /// <summary>
        /// Xoay vector theo góc độ (degrees).
        /// </summary>
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            return v.RotateRadians(degrees * Mathf.Deg2Rad);
        }

        /// <summary>
        /// Xoay vector theo góc radian.
        /// </summary>
        public static Vector2 RotateRadians(this Vector2 v, float radians)
        {
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
        }

        /// <summary>
        /// Trả về góc từ vector theo hệ tọa độ (0~360 độ).
        /// </summary>
        public static float ToAngle(this Vector2 v)
        {
            float angle = Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg;
            return angle < 0f ? angle + 360f : angle;
        }

        /// <summary>
        /// Random số thực trong khoảng x ~ y.
        /// </summary>
        public static float RandomWithin(this Vector2 v)
        {
            return Random.Range(v.x, v.y);
        }

        /// <summary>
        /// Random số nguyên trong khoảng x ~ y (đã làm tròn).
        /// </summary>
        public static int RandomIntWithin(this Vector2 v)
        {
            return Random.Range(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
        }

        /// <summary>
        /// Chiếu vector lên 1 hướng tuyến tính.
        /// </summary>
        public static Vector2 ProjectOnLine(this Vector2 vectorToProject, Vector2 line)
        {
            float dot = Vector2.Dot(vectorToProject, line);
            float magSq = line.sqrMagnitude;
            return (dot / magSq) * line;
        }

        /// <summary>
        /// Kiểm tra giá trị nằm trong khoảng (inclusive).
        /// </summary>
        public static bool InInside(this Vector2 v2, float value)
        {
            return value >= v2.x && value <= v2.y;
        }

        /// <summary>
        /// Kiểm tra giá trị nằm ngoài khoảng.
        /// </summary>
        public static bool IsOutside(this Vector2 v2, float value)
        {
            return value <= v2.x || value >= v2.y;
        }

        /// <summary>
        /// Chuyển sang Vector3 nằm trên mặt phẳng XY.
        /// </summary>
        public static Vector3 ToV3PlaneXY(this Vector2 v2)
        {
            return new Vector3(v2.x, v2.y, 0f);
        }

        /// <summary>
        /// Chuyển sang Vector3 nằm trên mặt phẳng XZ.
        /// </summary>
        public static Vector3 ToV3PlaneXZ(this Vector2 v2)
        {
            return new Vector3(v2.x, 0f, v2.y);
        }

        /// <summary>
        /// Chuyển sang Vector3 nằm trên mặt phẳng YZ.
        /// </summary>
        public static Vector3 ToV3PlaneYZ(this Vector2 v2)
        {
            return new Vector3(0f, v2.x, v2.y);
        }

        /// <summary>
        /// Clamp vector từng trục trong khoảng giới hạn min/max.
        /// </summary>
        public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max)
        {
            return new Vector2(
                Mathf.Clamp(v.x, min.x, max.x),
                Mathf.Clamp(v.y, min.y, max.y)
            );
        }

        /// <summary>
        /// Trả về vector có trị tuyệt đối của từng thành phần.
        /// </summary>
        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }

        /// <summary>
        /// Trả về vector ngược dấu.
        /// </summary>
        public static Vector2 Inverse(this Vector2 v)
        {
            return new Vector2(-v.x, -v.y);
        }

        /// <summary>
        /// Làm tròn vector đến step gần nhất (snap).
        /// </summary>
        public static Vector2 Snap(this Vector2 v, float step)
        {
            return new Vector2(
                Mathf.Round(v.x / step) * step,
                Mathf.Round(v.y / step) * step
            );
        }

        /// <summary>
        /// Kiểm tra vector có gần bằng Vector2.zero không (dưới epsilon).
        /// </summary>
        public static bool IsZero(this Vector2 v, float epsilon = 0.0001f)
        {
            return v.sqrMagnitude < epsilon * epsilon;
        }

        /// <summary>
        /// Trả về trục có giá trị lớn hơn (x hoặc y).
        /// </summary>
        public static string MaxAxis(this Vector2 v)
        {
            return Mathf.Abs(v.x) >= Mathf.Abs(v.y) ? "X" : "Y";
        }

        /// <summary>
        /// Trả về trục có giá trị nhỏ hơn (x hoặc y).
        /// </summary>
        public static string MinAxis(this Vector2 v)
        {
            return Mathf.Abs(v.x) <= Mathf.Abs(v.y) ? "X" : "Y";
        }
        
        // So sánh gần đúng giữa 2 Vector2
        public static bool Approximately(this Vector2 a, Vector2 b, float tolerance)
        {
            return (a - b).sqrMagnitude <= tolerance * tolerance;
        }
    }
}
