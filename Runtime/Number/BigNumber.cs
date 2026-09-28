using System;
using System.Globalization;
using SketchEngine.Utilities.Utils;

namespace SketchEngine.Number
{
    /// <summary>
    /// BigNumber for idle / incremental games.
    /// Representation: mantissa * 1000^exponent
    /// - mantissa in [1 .. 1000)
    /// - exponent can be very large (infinite units)
    /// </summary>
    [Serializable]
    public struct BigNumber : IComparable<BigNumber>
    {
        public double mantissa; // [1..1000)
        public int exponent;    // power of 1000

        // =====================================================
        // CONSTRUCTORS
        // =====================================================

        public BigNumber(double value)
        {
            if (value <= 0)
            {
                mantissa = 0;
                exponent = 0;
                return;
            }

            mantissa = value;
            exponent = 0;
            Normalize();
        }

        public BigNumber(double mantissa, int exponent)
        {
            this.mantissa = mantissa;
            this.exponent = exponent;
            Normalize();
        }

        // =====================================================
        // NORMALIZE
        // =====================================================

        void Normalize()
        {
            if (mantissa <= 0)
            {
                mantissa = 0;
                exponent = 0;
                return;
            }

            while (mantissa >= 1000.0)
            {
                mantissa /= 1000.0;
                exponent++;
            }

            while (mantissa < 1.0)
            {
                mantissa *= 1000.0;
                exponent--;
            }
        }

        // =====================================================
        // OPERATORS
        // =====================================================

        public static BigNumber operator +(BigNumber a, BigNumber b)
        {
            if (a.mantissa == 0) return b;
            if (b.mantissa == 0) return a;

            return a.exponent >= b.exponent
                ? AddInternal(a, b)
                : AddInternal(b, a);
        }

        static BigNumber AddInternal(BigNumber big, BigNumber small)
        {
            int diff = big.exponent - small.exponent;

            // Chênh lệch quá lớn → bỏ qua số nhỏ (chuẩn idle game)
            if (diff > 6)
                return big;

            double scaled = small.mantissa / Pow1000(diff);
            return new BigNumber(big.mantissa + scaled, big.exponent);
        }

        public static BigNumber operator -(BigNumber a, BigNumber b)
        {
            if (a <= b)
                return Zero;

            int diff = a.exponent - b.exponent;
            if (diff > 6)
                return a;

            double scaled = b.mantissa / Pow1000(diff);
            return new BigNumber(a.mantissa - scaled, a.exponent);
        }

        public static BigNumber operator *(BigNumber a, double multiplier)
        {
            if (multiplier <= 0)
                return Zero;

            return new BigNumber(a.mantissa * multiplier, a.exponent);
        }

        public static BigNumber operator /(BigNumber a, double divisor)
        {
            if (divisor <= 0)
                return Zero;

            return new BigNumber(a.mantissa / divisor, a.exponent);
        }

        // =====================================================
        // COMPARISON
        // =====================================================

        public int CompareTo(BigNumber other)
        {
            if (exponent != other.exponent)
                return exponent.CompareTo(other.exponent);

            return mantissa.CompareTo(other.mantissa);
        }

        public static bool operator >(BigNumber a, BigNumber b) => a.CompareTo(b) > 0;
        public static bool operator <(BigNumber a, BigNumber b) => a.CompareTo(b) < 0;
        public static bool operator >=(BigNumber a, BigNumber b) => a.CompareTo(b) >= 0;
        public static bool operator <=(BigNumber a, BigNumber b) => a.CompareTo(b) <= 0;

        // =====================================================
        // FORMAT
        // =====================================================

        public string ToCompactString(int decimalDigits = 2)
        {
            if (mantissa == 0)
                return "0";

            string unit = GetUnitString(exponent);

            return mantissa.ToString(
                       $"0.{new string('#', decimalDigits)}",
                       CultureInfo.InvariantCulture
                   ) + unit;
        }

        static string GetUnitString(int unitIndex)
        {
            // Base units
            if (unitIndex == 0) return "";
            if (unitIndex == 1) return "K";
            if (unitIndex == 2) return "M";
            if (unitIndex == 3) return "B";
            if (unitIndex == 4) return "T";

            // Extended units: aa, ab, zz, aaa...
            return ToExtendedUnit(unitIndex - 5);
        }

        static string ToExtendedUnit(int index)
        {
            string result = "";
            do
            {
                result = (char)('a' + (index % 26)) + result;
                index = index / 26 - 1;
            }
            while (index >= 0);
            return result;
        }

        // =====================================================
        // UTIL
        // =====================================================

        static double Pow1000(int exp)
        {
            double result = 1;
            for (int i = 0; i < exp; i++)
                result *= 1000.0;
            return result;
        }

        // =====================================================
        // STATIC
        // =====================================================

        public static BigNumber Zero => new BigNumber(0);

        public override string ToString() => ToCompactString();

        // =====================================================
        // PARSE
        // =====================================================

        public static BigNumber FromString(string value)
        {
            double parsed = UtilsNumber.ParseCompactNumber(value);
            return new BigNumber(parsed);
        }
    }

    // =========================================================
    // STRING EXTENSION
    // =========================================================

    public static class CSkBigNumberExtensions
    {
        /// <summary>
        /// Convert compact string (e.g. "2.5aa") to CSkBigNumber.
        /// Zero-GC friendly (struct return).
        /// </summary>
        public static BigNumber ToBigNumber(this string value)
        {
            return BigNumber.FromString(value);
        }
    }
}
