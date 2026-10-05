using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SketchEngine.Utilities
{
    /// <summary>
    /// Tiện ích toán không dùng LINQ, boxing hay bộ nhớ tạm trong đường chạy thông thường.
    /// Hàm số thực truyền NaN theo phép toán chuẩn, trừ các hàm Finite/Safe có quy định riêng.
    /// Lerp, InverseLerp và Remap giữ hành vi không clamp của API cũ; dùng hậu tố Clamped khi cần.
    /// </summary>
    public static class UtilityMath
    {
        public const float Epsilon = 1e-6f;
        public const double EpsilonDouble = 1e-12;
        public const double Tau = Math.PI * 2.0;
        public const double Deg2Rad = Math.PI / 180.0;
        public const double Rad2Deg = 180.0 / Math.PI;

        #region Giới hạn và kiểm tra

        /// <summary>Giới hạn giá trị; yêu cầu min ≤ max. Không tự đổi thứ tự để tránh che lỗi caller.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max) => value < min ? min : value > max ? max : value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Clamp(long value, long min, long max) => value < min ? min : value > max ? max : value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float value) => Clamp(value, 0f, 1f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp01(double value) => Clamp(value, 0.0, 1.0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        public static float FiniteOr(float value, float fallback = 0f) => IsFinite(value) ? value : fallback;
        public static double FiniteOr(double value, double fallback = 0.0) => IsFinite(value) ? value : fallback;

        /// <summary>So sánh bằng sai số tuyệt đối và tương đối không âm; NaN không bằng bất kỳ giá trị nào.</summary>
        public static bool Approximately(double a, double b, double absoluteTolerance = EpsilonDouble, double relativeTolerance = EpsilonDouble)
        {
            if (a == b) return true;
            if (!IsFinite(a) || !IsFinite(b)) return false;
            return Math.Abs(a - b) <= Math.Max(absoluteTolerance, relativeTolerance * Math.Max(Math.Abs(a), Math.Abs(b)));
        }
        public static bool Approximately(float a, float b, float absoluteTolerance = Epsilon, float relativeTolerance = Epsilon)
            => Approximately((double)a, b, absoluteTolerance, relativeTolerance);
        public static bool IsNearZero(float value, float tolerance = Epsilon) => Math.Abs(value) <= tolerance;
        public static bool IsNearZero(double value, double tolerance = EpsilonDouble) => Math.Abs(value) <= tolerance;
        public static bool InRange(float value, float min, float max) => value >= min && value <= max;
        public static bool InRange(double value, double min, double max) => value >= min && value <= max;
        public static bool InRange(int value, int min, int max) => value >= min && value <= max;

        #endregion

        #region Nội suy và đổi khoảng

        /// <summary>Nội suy không clamp; t ngoài [0..1] tạo ngoại suy. a, b nên hữu hạn.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Lerp(double a, double b, double t) => t == 0.0 ? a : t == 1.0 ? b : a * (1.0 - t) + b * t;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float t) => t == 0f ? a : t == 1f ? b : a * (1f - t) + b * t;
        public static double LerpUnclamped(double a, double b, double t) => Lerp(a, b, t);
        public static float LerpUnclamped(float a, float b, float t) => Lerp(a, b, t);
        public static double LerpClamped(double a, double b, double t) => Lerp(a, b, Clamp01(t));
        public static float LerpClamped(float a, float b, float t) => Lerp(a, b, Clamp01(t));

        /// <summary>Trả tỷ lệ không clamp; a == b trả 0. Hỗ trợ khoảng đảo chiều.</summary>
        public static double InverseLerp(double a, double b, double value)
        {
            if (a == b) return 0.0;
            double range = b - a;
            // Tránh tràn khi hai đầu hữu hạn nhưng cách nhau hơn double.MaxValue.
            return double.IsInfinity(range) && IsFinite(a) && IsFinite(b) && IsFinite(value)
                ? (value * 0.5 - a * 0.5) / (b * 0.5 - a * 0.5) : (value - a) / range;
        }
        public static float InverseLerp(float a, float b, float value) => (float)InverseLerp((double)a, b, value);
        public static double InverseLerpClamped(double a, double b, double value) => Clamp01(InverseLerp(a, b, value));
        public static float InverseLerpClamped(float a, float b, float value) => Clamp01(InverseLerp(a, b, value));
        public static double Remap(double fromMin, double fromMax, double toMin, double toMax, double value)
            => Lerp(toMin, toMax, InverseLerp(fromMin, fromMax, value));
        public static float Remap(float fromMin, float fromMax, float toMin, float toMax, float value)
            => Lerp(toMin, toMax, InverseLerp(fromMin, fromMax, value));
        public static double RemapClamped(double fromMin, double fromMax, double toMin, double toMax, double value)
            => Lerp(toMin, toMax, InverseLerpClamped(fromMin, fromMax, value));
        public static float RemapClamped(float fromMin, float fromMax, float toMin, float toMax, float value)
            => Lerp(toMin, toMax, InverseLerpClamped(fromMin, fromMax, value));
        /// <summary>Tỷ lệ đã clamp; max == 0 trả 0. Cho phép max âm để mô tả khoảng đảo chiều.</summary>
        public static double To01(double value, double max) => max == 0.0 ? 0.0 : Clamp01(value / max);
        public static float To01(float value, float max) => max == 0f ? 0f : Clamp01(value / max);
        public static float To01F(double value, double max) => (float)To01(value, max);

        /// <summary>Đường cong bậc ba, clamp t vào [0..1].</summary>
        public static float SmoothStep01(float t) { t = Clamp01(t); return t * t * (3f - 2f * t); }
        public static double SmoothStep01(double t) { t = Clamp01(t); return t * t * (3.0 - 2.0 * t); }
        /// <summary>Đường cong bậc năm; đạo hàm bậc một và hai bằng 0 tại hai đầu.</summary>
        public static float SmootherStep01(float t) { t = Clamp01(t); return t * t * t * (t * (t * 6f - 15f) + 10f); }
        public static double SmootherStep01(double t) { t = Clamp01(t); return t * t * t * (t * (t * 6.0 - 15.0) + 10.0); }
        public static float SmoothStep(float from, float to, float t) => Lerp(from, to, SmoothStep01(t));
        public static double SmoothStep(double from, double to, double t) => Lerp(from, to, SmoothStep01(t));

        #endregion

        #region Chia an toàn, làm tròn và bước nhảy

        /// <summary>Trả fallback nếu đầu vào hoặc kết quả không hữu hạn, hay |mẫu số| ≤ tolerance không âm.</summary>
        public static double DivSafe(double a, double b, double fallback = 0.0, double tolerance = 0.0)
        {
            if (!IsFinite(a) || !IsFinite(b) || Math.Abs(b) <= tolerance) return fallback;
            return FiniteOr(a / b, fallback);
        }
        public static float DivSafe(float a, float b, float fallback = 0f, float tolerance = 0f)
        {
            if (!IsFinite(a) || !IsFinite(b) || Math.Abs(b) <= tolerance) return fallback;
            return FiniteOr(a / b, fallback);
        }
        /// <summary>Tránh chia 0 và trường hợp int.MinValue / -1 gây tràn.</summary>
        public static int DivSafe(int a, int b, int fallback = 0) => b == 0 || (a == int.MinValue && b == -1) ? fallback : a / b;
        public static long DivSafe(long a, long b, long fallback = 0) => b == 0 || (a == long.MinValue && b == -1) ? fallback : a / b;
        /// <summary>digits trong [0..15]; mặc định làm tròn về số chẵn tại điểm giữa, tương thích API cũ.</summary>
        public static double RoundTo(double value, int digits) => Math.Round(value, digits);
        public static double RoundTo(double value, int digits, MidpointRounding mode) => Math.Round(value, digits, mode);
        /// <summary>Làm tròn tới bội step dương; step không hợp lệ trả nguyên value.</summary>
        public static double RoundToStep(double value, double step, MidpointRounding mode = MidpointRounding.AwayFromZero)
            => step > 0.0 && IsFinite(step) ? Math.Round(value / step, mode) * step : value;
        public static float RoundToStep(float value, float step) => (float)RoundToStep((double)value, step);
        public static double FloorToStep(double value, double step) => step > 0.0 && IsFinite(step) ? Math.Floor(value / step) * step : value;
        public static double CeilToStep(double value, double step) => step > 0.0 && IsFinite(step) ? Math.Ceiling(value / step) * step : value;
        /// <summary>Đưa giá trị vào vùng chết quanh 0; không đổi tỷ lệ phần còn lại.</summary>
        public static float DeadZone(float value, float threshold) => Math.Abs(value) <= threshold ? 0f : value;

        #endregion

        #region Tuần hoàn và góc

        /// <summary>Modulo không âm với length dương; đầu vào không hợp lệ trả 0.</summary>
        public static double Repeat(double value, double length)
        {
            if (!(length > 0.0) || !IsFinite(length) || !IsFinite(value)) return 0.0;
            double result = value % length;
            if (result < 0.0) result += length;
            return result >= length ? 0.0 : result;
        }
        public static float Repeat(float value, float length)
        {
            float result = (float)Repeat((double)value, length);
            return result >= length ? 0f : result;
        }
        /// <summary>Wrap vào [min..max), yêu cầu hai đầu hữu hạn và min &lt; max.</summary>
        public static double Wrap(double value, double min, double max) => min + Repeat(value - min, max - min);
        public static float Wrap(float value, float min, float max) => (float)Wrap((double)value, min, max);
        /// <summary>Modulo nguyên không âm; modulus ≤ 0 trả 0.</summary>
        public static int Mod(int value, int modulus)
        {
            if (modulus <= 0) return 0;
            int result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
        public static long Mod(long value, long modulus)
        {
            if (modulus <= 0) return 0;
            long result = value % modulus;
            return result < 0 ? result + modulus : result;
        }
        public static double PingPong(double value, double length)
        {
            if (!(length > 0.0) || !IsFinite(length)) return 0.0;
            if (length > double.MaxValue * 0.5)
            {
                double half = Repeat(value * 0.5, length);
                return half <= length * 0.5 ? half * 2.0 : (length - half) * 2.0;
            }
            return length - Math.Abs(Repeat(value, length * 2.0) - length);
        }
        public static float PingPong(float value, float length) => (float)PingPong((double)value, length);
        public static double NormalizeAngle(double degrees) => Repeat(degrees, 360.0);
        public static float NormalizeAngle(float degrees) => Repeat(degrees, 360f);
        /// <summary>Khoảng cách góc có dấu ngắn nhất, trong (-180..180]; đơn vị độ.</summary>
        public static double DeltaAngle(double current, double target)
        {
            double delta = Repeat(target - current, 360.0);
            return delta > 180.0 ? delta - 360.0 : delta;
        }
        public static float DeltaAngle(float current, float target) => (float)DeltaAngle((double)current, target);
        public static float LerpAngle(float from, float to, float t) => from + DeltaAngle(from, to) * Clamp01(t);
        public static double LerpAngle(double from, double to, double t) => from + DeltaAngle(from, to) * Clamp01(t);

        #endregion

        #region Di chuyển và làm mượt

        /// <summary>Tiến tới target tối đa maxDelta không âm; không vượt target.</summary>
        public static double MoveTowards(double current, double target, double maxDelta)
        {
            if (maxDelta <= 0.0 || current == target) return current;
            double delta = target - current;
            if (Math.Abs(delta) <= maxDelta) return target;
            return current + (delta > 0.0 ? maxDelta : -maxDelta);
        }
        public static float MoveTowards(float current, float target, float maxDelta) => (float)MoveTowards((double)current, target, maxDelta);
        public static float MoveTowardsAngle(float current, float target, float maxDelta)
            => MoveTowards(current, current + DeltaAngle(current, target), maxDelta);
        public static double MoveTowardsAngle(double current, double target, double maxDelta)
            => MoveTowards(current, current + DeltaAngle(current, target), maxDelta);
        /// <summary>Hệ số làm mượt mũ; sharpness và deltaTime không âm. Cache hệ số khi dùng chung cho nhiều đối tượng.</summary>
        public static double DampFactor(double sharpness, double deltaTime)
            => sharpness <= 0.0 || deltaTime <= 0.0 ? 0.0 : 1.0 - Math.Exp(-sharpness * deltaTime);
        public static float DampFactor(float sharpness, float deltaTime) => (float)DampFactor((double)sharpness, deltaTime);
        public static float Damp(float current, float target, float sharpness, float deltaTime)
            => Lerp(current, target, DampFactor(sharpness, deltaTime));
        public static double Damp(double current, double target, double sharpness, double deltaTime)
            => Lerp(current, target, DampFactor(sharpness, deltaTime));
        /// <summary>Hệ số theo chu kỳ bán rã: sau halfLife giây, khoảng cách tới đích giảm một nửa.</summary>
        public static double HalfLifeFactor(double halfLife, double deltaTime)
            => deltaTime <= 0.0 ? 0.0 : halfLife <= 0.0 ? 1.0 : 1.0 - Math.Exp(-0.6931471805599453 * deltaTime / halfLife);
        public static float HalfLifeFactor(float halfLife, float deltaTime) => (float)HalfLifeFactor((double)halfLife, deltaTime);
        public static float DampHalfLife(float current, float target, float halfLife, float deltaTime)
            => Lerp(current, target, HalfLifeFactor(halfLife, deltaTime));
        public static double DampHalfLife(double current, double target, double halfLife, double deltaTime)
            => Lerp(current, target, HalfLifeFactor(halfLife, deltaTime));
        public static float DampAngle(float current, float target, float sharpness, float deltaTime)
            => current + DeltaAngle(current, target) * DampFactor(sharpness, deltaTime);

        #endregion

        #region Vector và hình học

        public static Vector2 Lerp(Vector2 from, Vector2 to, float t) => new Vector2(Lerp(from.x, to.x, t), Lerp(from.y, to.y, t));
        public static Vector3 Lerp(Vector3 from, Vector3 to, float t) => new Vector3(Lerp(from.x, to.x, t), Lerp(from.y, to.y, t), Lerp(from.z, to.z, t));
        public static Vector2 Damp(Vector2 current, Vector2 target, float sharpness, float deltaTime) => Lerp(current, target, DampFactor(sharpness, deltaTime));
        public static Vector3 Damp(Vector3 current, Vector3 target, float sharpness, float deltaTime) => Lerp(current, target, DampFactor(sharpness, deltaTime));
        public static Vector2 DampHalfLife(Vector2 current, Vector2 target, float halfLife, float deltaTime) => Lerp(current, target, HalfLifeFactor(halfLife, deltaTime));
        public static Vector3 DampHalfLife(Vector3 current, Vector3 target, float halfLife, float deltaTime) => Lerp(current, target, HalfLifeFactor(halfLife, deltaTime));
        public static float SqrDistance(Vector2 a, Vector2 b) => (a - b).sqrMagnitude;
        public static float SqrDistance(Vector3 a, Vector3 b) => (a - b).sqrMagnitude;
        /// <summary>Kiểm tra bán kính không cần căn bậc hai; radius âm trả false.</summary>
        public static bool WithinDistance(Vector2 a, Vector2 b, float radius) => radius >= 0f && SqrDistance(a, b) <= radius * radius;
        public static bool WithinDistance(Vector3 a, Vector3 b, float radius) => radius >= 0f && SqrDistance(a, b) <= radius * radius;
        /// <summary>Chuẩn hóa vector hữu hạn; độ dài ≤ tolerance trả fallback. Không tràn bình phương khi vector lớn.</summary>
        public static Vector2 NormalizeSafe(Vector2 value, Vector2 fallback = default, float tolerance = Epsilon)
        {
            if (!IsFinite(value.x) || !IsFinite(value.y)) return fallback;
            double length = Math.Sqrt((double)value.x * value.x + (double)value.y * value.y);
            return length <= Math.Max(0f, tolerance) ? fallback : new Vector2((float)(value.x / length), (float)(value.y / length));
        }
        public static Vector3 NormalizeSafe(Vector3 value, Vector3 fallback = default, float tolerance = Epsilon)
        {
            if (!IsFinite(value.x) || !IsFinite(value.y) || !IsFinite(value.z)) return fallback;
            double length = Math.Sqrt((double)value.x * value.x + (double)value.y * value.y + (double)value.z * value.z);
            return length <= Math.Max(0f, tolerance) ? fallback : new Vector3((float)(value.x / length), (float)(value.y / length), (float)(value.z / length));
        }
        public static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
        public static Vector2 Perpendicular(Vector2 value) => new Vector2(-value.y, value.x);
        /// <summary>Xoay vector 2D ngược chiều kim đồng hồ, đơn vị độ.</summary>
        public static Vector2 Rotate(Vector2 value, float degrees)
        {
            double angle = degrees * Deg2Rad;
            float sin = (float)Math.Sin(angle), cos = (float)Math.Cos(angle);
            return new Vector2(value.x * cos - value.y * sin, value.x * sin + value.y * cos);
        }
        /// <summary>Điểm gần nhất trên đoạn thẳng; đoạn có độ dài 0 trả a.</summary>
        public static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var delta = b - a;
            float lengthSq = delta.sqrMagnitude;
            return lengthSq == 0f ? a : a + delta * Clamp01(Vector2.Dot(point - a, delta) / lengthSq);
        }
        public static Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            var delta = b - a;
            float lengthSq = delta.sqrMagnitude;
            return lengthSq == 0f ? a : a + delta * Clamp01(Vector3.Dot(point - a, delta) / lengthSq);
        }

        public static float SqrDistanceToSegment(Vector2 point, Vector2 a, Vector2 b) => SqrDistance(point, ClosestPointOnSegment(point, a, b));
        public static float SqrDistanceToSegment(Vector3 point, Vector3 a, Vector3 b) => SqrDistance(point, ClosestPointOnSegment(point, a, b));
        /// <summary>Chiếu vector lên hướng normal; normal nên hữu hạn. Vector chuẩn hóa vẫn hoạt động với độ lớn rất lớn.</summary>
        public static Vector3 ProjectSafe(Vector3 value, Vector3 normal)
        {
            var unit = NormalizeSafe(normal);
            return unit * Vector3.Dot(value, unit);
        }
        public static Vector3 ProjectOnPlaneSafe(Vector3 value, Vector3 normal) => value - ProjectSafe(value, normal);
        /// <summary>Điểm trên đường cong Bézier bậc hai; t không clamp.</summary>
        public static Vector3 BezierQuadratic(Vector3 start, Vector3 control, Vector3 end, float t)
            => Lerp(Lerp(start, control, t), Lerp(control, end, t), t);
        /// <summary>Điểm trên đường cong Bézier bậc ba; t không clamp.</summary>
        public static Vector3 BezierCubic(Vector3 start, Vector3 controlA, Vector3 controlB, Vector3 end, float t)
            => Lerp(BezierQuadratic(start, controlA, controlB, t), BezierQuadratic(controlA, controlB, end, t), t);

        #endregion

        #region Số nguyên và lũy thừa

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Square(float value) => value * value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Square(double value) => value * value;
        public static float Cube(float value) => value * value * value;
        public static double Cube(double value) => value * value * value;
        public static bool IsPowerOfTwo(int value) => value > 0 && (value & (value - 1)) == 0;
        /// <summary>Lũy thừa 2 nhỏ nhất ≥ value; value ≤ 1 trả 1, vượt 2^30 trả 0 (không biểu diễn được bằng int dương).</summary>
        public static int NextPowerOfTwo(int value)
        {
            if (value <= 1) return 1;
            if (value > 0x40000000) return 0;
            uint bits = (uint)(value - 1);
            bits |= bits >> 1; bits |= bits >> 2; bits |= bits >> 4; bits |= bits >> 8; bits |= bits >> 16;
            return (int)(bits + 1);
        }
        /// <summary>Lũy thừa 2 lớn nhất ≤ value; value ≤ 0 trả 0.</summary>
        public static int PreviousPowerOfTwo(int value)
        {
            if (value <= 0) return 0;
            uint bits = (uint)value;
            bits |= bits >> 1; bits |= bits >> 2; bits |= bits >> 4; bits |= bits >> 8; bits |= bits >> 16;
            return (int)(bits - (bits >> 1));
        }
        /// <summary>Chia và làm tròn lên, hỗ trợ cả số âm. Mẫu 0 hoặc kết quả tràn ném exception.</summary>
        public static int DivideCeil(int value, int divisor)
        {
            long result = DivideCeil((long)value, divisor);
            return checked((int)result);
        }
        public static long DivideCeil(long value, long divisor)
        {
            long quotient = value / divisor, remainder = value % divisor;
            return remainder != 0 && (remainder > 0) == (divisor > 0) ? quotient + 1 : quotient;
        }
        /// <summary>Cộng/bớt rồi giới hạn tại int.MinValue/int.MaxValue khi tràn.</summary>
        public static int AddSaturating(int a, int b) => (int)Clamp((long)a + b, int.MinValue, int.MaxValue);
        public static int SubtractSaturating(int a, int b) => (int)Clamp((long)a - b, int.MinValue, int.MaxValue);
        public static int MultiplySaturating(int a, int b) => (int)Clamp((long)a * b, int.MinValue, int.MaxValue);
        /// <summary>Chẵn/lẻ hoạt động với cả số âm.</summary>
        public static bool IsEven(int value) => (value & 1) == 0;
        public static bool IsOdd(int value) => (value & 1) != 0;

        #endregion
    }
}
