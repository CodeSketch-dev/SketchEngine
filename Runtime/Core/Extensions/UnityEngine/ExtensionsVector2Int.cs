using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    /// <summary>
    /// Các hàm mở rộng tiện ích cho Vector2Int.
    /// </summary>
    public static class ExtensionsVector2Int 
    {
        /// <summary>
        /// Trả về một số nguyên ngẫu nhiên nằm trong khoảng (inclusiveMin, exclusiveMax) từ Vector2Int.
        /// </summary>
        public static int RandomWithin(this Vector2Int v)
        {
            return Random.Range(v.x, v.y);
        }

        /// <summary>
        /// Kiểm tra một giá trị int có nằm trong khoảng Vector2Int không (x <= value <= y).
        /// </summary>
        public static bool Contains(this Vector2Int range, int value)
        {
            return value >= range.x && value <= range.y;
        }

        /// <summary>
        /// Clamp một giá trị int vào khoảng Vector2Int.
        /// </summary>
        public static int Clamp(this Vector2Int range, int value)
        {
            return Mathf.Clamp(value, range.x, range.y);
        }

        /// <summary>
        /// Kiểm tra xem giá trị có nằm ngoài khoảng Vector2Int không.
        /// </summary>
        public static bool IsOutside(this Vector2Int range, int value)
        {
            return value < range.x || value > range.y;
        }

        /// <summary>
        /// Trả về độ dài khoảng (y - x).
        /// </summary>
        public static int Length(this Vector2Int range)
        {
            return Mathf.Abs(range.y - range.x);
        }

        /// <summary>
        /// Đổi Vector2Int thành Vector2.
        /// </summary>
        public static Vector2 ToVector2(this Vector2Int v)
        {
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        /// Đổi Vector2Int sang Vector3 trên mặt phẳng XY.
        /// </summary>
        public static Vector3 ToV3PlaneXY(this Vector2Int v)
        {
            return new Vector3(v.x, v.y, 0f);
        }

        /// <summary>
        /// Đổi Vector2Int sang Vector3 trên mặt phẳng XZ.
        /// </summary>
        public static Vector3 ToV3PlaneXZ(this Vector2Int v)
        {
            return new Vector3(v.x, 0f, v.y);
        }

        /// <summary>
        /// Đổi Vector2Int sang Vector3 trên mặt phẳng YZ.
        /// </summary>
        public static Vector3 ToV3PlaneYZ(this Vector2Int v)
        {
            return new Vector3(0f, v.x, v.y);
        }

        /// <summary>
        /// Lấy giá trị trung bình giữa x và y, làm tròn xuống.
        /// </summary>
        public static int Mid(this Vector2Int v)
        {
            return (v.x + v.y) / 2;
        }

        /// <summary>
        /// Kiểm tra xem khoảng có hợp lệ không (x <= y).
        /// </summary>
        public static bool IsValid(this Vector2Int v)
        {
            return v.x <= v.y;
        }
        
        // So sánh gần đúng giữa 2 Vector2Int
        public static bool Approximately(this Vector2Int a, Vector2Int b, float tolerance)
        {
            return (a - b).sqrMagnitude <= tolerance * tolerance;
        }
    }
}
