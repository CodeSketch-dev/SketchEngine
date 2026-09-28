using System;
using UnityEngine;

namespace SketchEngine.Utilities.Utils
{
    public static class UtilsMath
    {
        /// <summary>
        /// Tuyến tính nội suy giữa a và b theo t (giá trị giữa 0–1).
        /// </summary>
        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * t;
        }

        /// <summary>
        /// Lấy giá trị t (0–1) từ v nằm giữa a và b.
        /// </summary>
        public static double InverseLerp(double a, double b, double v)
        {
            if (Math.Abs(b - a) < double.Epsilon) return 0;
            return (v - a) / (b - a);
        }
        
        /// <summary>
        /// Tính tỉ lệ từ 0 → 1 của giá trị trong đoạn [0, max]. Có clamp.
        /// </summary>
        public static double To01(double value, double max)
        {
            if (Math.Abs(max) < double.Epsilon) return 0.0;
            return Clamp(value / max, 0.0, 1.0);
        }

        public static float To01F(double value, double max) => (float)To01(value, max);

        /// <summary>
        /// Nội suy lại giá trị từ khoảng [fromMin, fromMax] sang [toMin, toMax]
        /// </summary>
        public static double Remap(double fromMin, double fromMax, double toMin, double toMax, double value)
        {
            double t = InverseLerp(fromMin, fromMax, value);
            return Lerp(toMin, toMax, t);
        }

        /// <summary>
        /// Chia an toàn, tránh chia cho 0
        /// </summary>
        public static double DivSafe(double a, double b, double fallback = 0.0)
        {
            return Math.Abs(b) < double.Epsilon ? fallback : a / b;
        }

        /// <summary>
        /// Làm tròn tới chữ số thập phân nhất định
        /// </summary>
        public static double RoundTo(double value, int digits)
        {
            return Math.Round(value, digits);
        }
        
        /// <summary>
        /// Giới hạn giá trị trong khoảng [min, max]
        /// </summary>
        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        
        /// <summary>
        /// Giới hạn giá trị float trong khoảng [min, max]
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Giới hạn giá trị int trong khoảng [min, max]
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
