using System;
using System.Globalization;
using UnityEngine;
using SketchEngine.Diagnostics;

namespace SketchEngine.Utilities
{
    /// <summary>Compact display: K M B T a...z aa ab...zz aaa. B/b, K/k, M/m, T/t are distinct.</summary>
    public static class UtilityNumber
    {
        static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        static readonly CultureInfo VNDot = CreateCulture(".");
        static readonly CultureInfo VNComma = CreateCulture(",");
        static readonly string[] Formats = CreateFormats();
        static readonly string[] Separators = CreateSeparators();
        static CultureInfo CreateCulture(string separator)
        {
            var culture = new CultureInfo("vi-VN");
            culture.NumberFormat.NumberGroupSeparator = ".";
            culture.NumberFormat.NumberDecimalSeparator = separator;
            return culture;
        }
        static string[] CreateFormats()
        {
            var result = new string[16];
            result[0] = "0";
            for (int i = 1; i < result.Length; i++) result[i] = "0." + new string('#', i);
            return result;
        }
        static string[] CreateSeparators()
        {
            var result = new string[16];
            for (int i = 0; i < result.Length; i++) result[i] = "N" + i;
            return result;
        }
        static string DecimalFormat(int digits) => digits <= 0 ? Formats[0] : digits < Formats.Length ? Formats[digits] : "0." + new string('#', digits);
        // Keep CompactValue as the stored currency. Convert to text only when the displayed value changes.
        public static CompactValue ParseValue(string text) => CompactValue.Parse(text);
        public static bool TryParseValue(string text, out CompactValue value) => CompactValue.TryParse(text, out value);
        public static string Format(CompactValue value, int decimalDigits = 2) => value.ToCompactString(decimalDigits);
        public static bool TryFormat(CompactValue value, Span<char> destination, out int charsWritten, int decimalDigits = 2)
            => value.TryFormat(destination, out charsWritten, decimalDigits);
        public static string Save(CompactValue value) => value.ToSaveString();
        public static bool TryLoad(string saved, out CompactValue value) => CompactValue.TryParseSave(saved.AsSpan(), out value);

        public static string Format(float value) => Format((double)value);
        /// <summary>Rounds values below 1000 to whole numbers; larger values use compact suffixes.</summary>
        public static string Format(double value, int decimalDigits = 2)
        {
            int unit = Scale(ref value);
            return FormatMantissa(value, unit, decimalDigits);
        }
        static int Scale(ref double value)
        {
            int unit = 0;
            if (!double.IsInfinity(value) && !double.IsNaN(value))
                while (Math.Abs(value) >= 1000) { value /= 1000; unit++; }
            return unit;
        }
        /// <summary>Only allocates the final string at precision 0..15. Values without a suffix round to whole numbers.</summary>
        public static string FormatMantissa(double mantissa, int unitIndex, int decimalDigits = 2)
        {
            Span<char> buffer = stackalloc char[384];
            TryFormatMantissa(mantissa, unitIndex, buffer, out int written, decimalDigits);
            return new string(buffer.Slice(0, written));
        }
        /// <summary>No managed allocation for precision 0..15. False means insufficient space; buffer contents are unspecified.</summary>
        public static bool TryFormatMantissa(double mantissa, int unitIndex, Span<char> destination, out int charsWritten, int decimalDigits = 2)
        {
            charsWritten = 0;
            decimalDigits = Math.Min(Math.Max(decimalDigits, 0), 15);
            if (unitIndex <= 0)
            {
                mantissa = Math.Round(ScaleToDouble(mantissa, unitIndex));
                unitIndex = 0;
                if (Math.Abs(mantissa) < 1000 || double.IsNaN(mantissa) || double.IsInfinity(mantissa))
                    return mantissa.TryFormat(destination, out charsWritten, "0", Invariant);
                mantissa /= 1000;
                unitIndex = 1;
            }
            // Promote rounded boundaries (999.999K -> 1M), without overflowing the exponent.
            if (Math.Abs(Math.Round(mantissa, decimalDigits)) >= 1000 && unitIndex < int.MaxValue
                && !double.IsNaN(mantissa) && !double.IsInfinity(mantissa))
            {
                mantissa /= 1000;
                unitIndex++;
            }
            if (!mantissa.TryFormat(destination, out int count, DecimalFormat(decimalDigits), Invariant)) return false;
            int suffix = double.IsNaN(mantissa) || double.IsInfinity(mantissa) ? 0 : WriteSuffix(unitIndex, destination.Slice(count));
            if (suffix < 0) return false;
            charsWritten = count + suffix;
            return true;
        }
        public static bool TryFormat(double value, Span<char> destination, out int charsWritten, int decimalDigits = 2)
        {
            int unit = Scale(ref value);
            return TryFormatMantissa(value, unit, destination, out charsWritten, decimalDigits);
        }
        static int WriteSuffix(int unit, Span<char> destination)
        {
            if (unit <= 0) return 0;
            if (unit < 5)
            {
                if (destination.IsEmpty) return -1;
                destination[0] = " KMBT"[unit];
                return 1;
            }
            long value = (long)unit - 4;
            int length = 0;
            for (long remaining = value; remaining > 0; remaining = (remaining - 1) / 26) length++;
            if (destination.Length < length) return -1;
            for (int i = length - 1; i >= 0; i--)
            {
                value--;
                destination[i] = (char)('a' + value % 26);
                value /= 26;
            }
            return length;
        }
        public static float ParseCompactNumberFloat(string value) => (float)ParseCompactNumber(value);
        public static double ParseCompactNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            if (TryParseMantissa(value.AsSpan(), out double mantissa, out int exponent)) return ScaleToDouble(mantissa, exponent);
            if (double.TryParse(value, NumberStyles.Float, Invariant, out double plain)) return plain;
            SketchDebug.Log($"ParseCompactNumber: Cannot parse '{value}'", Color.red);
            return 0;
        }
        /// <summary>Parses directly into mantissa/exponent without substrings or converting large values to double.</summary>
        public static bool TryParseMantissa(ReadOnlySpan<char> text, out double mantissa, out int exponent)
        {
            mantissa = 0; exponent = 0;
            text = text.Trim();
            if (text.IsEmpty) return false;
            int end = text.Length;
            while (end > 0 && IsLetter(text[end - 1])) end--;
            if (!TryUnit(text.Slice(end), out int unit)) return false;
            ReadOnlySpan<char> number = text.Slice(0, end).Trim();
            int scientific = -1;
            for (int i = 0; i < number.Length; i++)
                if (number[i] == 'e' || number[i] == 'E') { scientific = i; break; }
            long power = unit;
            if (scientific >= 0)
            {
                if (!int.TryParse(number.Slice(scientific + 1), NumberStyles.AllowLeadingSign, Invariant, out int decimalPower)) return false;
                power += decimalPower / 3;
                if (!double.TryParse(number.Slice(0, scientific), NumberStyles.Float, Invariant, out mantissa)) return false;
                mantissa *= Math.Pow(10, decimalPower % 3);
            }
            else if (!double.TryParse(number, NumberStyles.Float, Invariant, out mantissa)) return false;
            if (double.IsInfinity(mantissa) || double.IsNaN(mantissa)) return false;
            if (mantissa == 0) return true;
            while (Math.Abs(mantissa) >= 1000) { mantissa /= 1000; power++; }
            while (Math.Abs(mantissa) < 1) { mantissa *= 1000; power--; }
            if (power < int.MinValue || power > int.MaxValue) { mantissa = 0; return false; }
            exponent = (int)power;
            return true;
        }
        static bool IsLetter(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        static bool TryUnit(ReadOnlySpan<char> suffix, out int unit)
        {
            unit = 0;
            if (suffix.IsEmpty) return true;
            if (suffix.Length == 1)
            {
                switch (suffix[0])
                {
                    case 'K': unit = 1; return true;
                    case 'M': unit = 2; return true;
                    case 'B': unit = 3; return true;
                    case 'T': unit = 4; return true;
                }
            }
            long index = 0;
            foreach (char c in suffix)
            {
                char letter = c >= 'A' && c <= 'Z' ? (char)(c + 32) : c;
                if (letter < 'a' || letter > 'z') return false;
                index = index * 26 + letter - 'a' + 1;
                if (index > int.MaxValue - 4L) return false;
            }
            unit = (int)index + 4;
            return true;
        }
        internal static double ScaleToDouble(double mantissa, int exponent)
        {
            if (mantissa == 0) return 0;
            if (exponent > 102) return mantissa > 0 ? double.PositiveInfinity : double.NegativeInfinity;
            if (exponent < -108) return 0;
            return exponent < 0 ? mantissa * Math.Pow(1000, exponent + 1) / 1000 : mantissa * Math.Pow(1000, exponent);
        }
        public static string FormatPriceVN(int value) => value.ToString("N0", VNDot);
        public static string FormatWithSeparator(double value) => value.ToString("N0", VNComma);
        public static string FormatWithSeparator(double value, int decimalDigits)
            => value.ToString(decimalDigits >= 0 && decimalDigits < Separators.Length ? Separators[decimalDigits] : "N" + decimalDigits, VNComma);
        public static string FormatWithSeparator(float value) => FormatWithSeparator((double)value);
    }
    public static class CompactNumberMath
    {
        static CompactValue ParseOrZero(string text) => CompactValue.TryParse(text, out var value) ? value : CompactValue.Zero;
        static string Display(CompactValue value) => value.ToCompactString();
        public static string Add(string a, string b) => Display(ParseOrZero(a) + ParseOrZero(b));
        public static string Subtract(string a, string b)
        {
            var result = ParseOrZero(a) - ParseOrZero(b);
            return result.Mantissa <= 0 ? "0" : Display(result);
        }
        public static string Multiply(string a, float multiplier) => Multiply(a, (double)multiplier);
        public static string Multiply(string a, double multiplier) => Display(ParseOrZero(a) * new CompactValue(multiplier));
        public static string Multiply(string a, string b) => Display(ParseOrZero(a) * ParseOrZero(b));
        public static string Divide(string a, float divisor) => Divide(a, (double)divisor);
        public static string Divide(string a, double divisor) => divisor <= 0 ? "0" : Display(ParseOrZero(a) / new CompactValue(divisor));
        public static string Divide(string a, string b)
        {
            var divisor = ParseOrZero(b);
            return divisor.Mantissa <= 0 ? "0" : Display(ParseOrZero(a) / divisor);
        }
        public static bool GreaterThan(string a, string b) => ParseOrZero(a).CompareTo(ParseOrZero(b)) > 0;
    }
}

namespace SketchEngine.Utilities
{
    /// <summary>
    /// Signed fixed-cost idle-game arithmetic: mantissa * 1000^exponent.
    /// Approximately 15-16 significant digits, checked Int32 exponent; not arbitrary-precision accounting.
    /// Immutable, allocation-free arithmetic and span parsing/formatting after initialization.
    /// </summary>
    [Serializable]
    public readonly struct CompactValue : IComparable<CompactValue>, IEquatable<CompactValue>
    {
        public double Mantissa { get; }
        public int Exponent { get; }
        static readonly double[] Powers = { 1, 1000, 1e6, 1e9, 1e12, 1e15, 1e18 };
        public CompactValue(double value) : this(value, 0) { }
        public CompactValue(double mantissa, int exponent) : this(mantissa, (long)exponent) { }
        CompactValue(double mantissa, long exponent)
        {
            if (double.IsNaN(mantissa) || double.IsInfinity(mantissa))
                throw new ArgumentOutOfRangeException(nameof(mantissa), "A finite mantissa is required.");
            if (mantissa == 0) { Mantissa = 0; Exponent = 0; return; }
            while (Math.Abs(mantissa) >= 1000) { mantissa /= 1000; exponent++; }
            while (Math.Abs(mantissa) < 1) { mantissa *= 1000; exponent--; }
            if (exponent < int.MinValue || exponent > int.MaxValue) throw new OverflowException("Compact number exponent exceeded Int32 range.");
            Mantissa = mantissa; Exponent = (int)exponent;
        }
        public static CompactValue Zero => default;
        public static CompactValue One => new CompactValue(1);
        public static implicit operator CompactValue(double value) => new CompactValue(value);
        public static CompactValue Min(CompactValue a, CompactValue b) => a <= b ? a : b;
        public static CompactValue Max(CompactValue a, CompactValue b) => a >= b ? a : b;
        public static CompactValue Abs(CompactValue value) => value.Mantissa < 0 ? -value : value;
        /// <summary>Clamped interpolation for float-progress tweens; never converts currency to double.</summary>
        public static CompactValue Lerp(CompactValue start, CompactValue end, double t)
        {
            if (double.IsNaN(t)) throw new ArgumentOutOfRangeException(nameof(t));
            if (t <= 0) return start;
            if (t >= 1) return end;
            return start + (end - start) * t;
        }
        public static CompactValue operator +(CompactValue a, CompactValue b)
        {
            if (a.Mantissa == 0) return b;
            if (b.Mantissa == 0) return a;
            if (a.Exponent < b.Exponent) { var swap = a; a = b; b = swap; }
            long difference = (long)a.Exponent - b.Exponent;
            if (difference > 6) return a;
            return new CompactValue(a.Mantissa + b.Mantissa / Powers[(int)difference], a.Exponent);
        }
        public static CompactValue operator -(CompactValue a) => new CompactValue(-a.Mantissa, a.Exponent);
        public static CompactValue operator -(CompactValue a, CompactValue b) => a + (-b);
        public static CompactValue operator *(CompactValue a, CompactValue b)
            => new CompactValue(a.Mantissa * b.Mantissa, (long)a.Exponent + b.Exponent);
        public static CompactValue operator /(CompactValue a, CompactValue b)
        {
            if (b.Mantissa == 0) throw new DivideByZeroException();
            return new CompactValue(a.Mantissa / b.Mantissa, (long)a.Exponent - b.Exponent);
        }
        public int CompareTo(CompactValue other)
        {
            int sign = Math.Sign(Mantissa), otherSign = Math.Sign(other.Mantissa);
            if (sign != otherSign) return sign.CompareTo(otherSign);
            if (sign == 0) return 0;
            int power = Exponent.CompareTo(other.Exponent);
            return power != 0 ? power * sign : Mantissa.CompareTo(other.Mantissa);
        }
        public bool Equals(CompactValue other) => Mantissa == other.Mantissa && Exponent == other.Exponent;
        public override bool Equals(object obj) => obj is CompactValue other && Equals(other);
        public override int GetHashCode() => unchecked(Mantissa.GetHashCode() * 397 ^ Exponent);
        public static bool operator ==(CompactValue a, CompactValue b) => a.Equals(b);
        public static bool operator !=(CompactValue a, CompactValue b) => !a.Equals(b);
        public static bool operator >(CompactValue a, CompactValue b) => a.CompareTo(b) > 0;
        public static bool operator <(CompactValue a, CompactValue b) => a.CompareTo(b) < 0;
        public static bool operator >=(CompactValue a, CompactValue b) => a.CompareTo(b) >= 0;
        public static bool operator <=(CompactValue a, CompactValue b) => a.CompareTo(b) <= 0;
        /// <summary>Conversion may overflow to infinity or underflow to zero; arithmetic does not use this conversion.</summary>
        public double ToDouble() => UtilityNumber.ScaleToDouble(Mantissa, Exponent);
        public bool TryToDouble(out double value)
        {
            value = ToDouble();
            return !double.IsInfinity(value) && (value != 0 || Mantissa == 0);
        }
        public static bool TryParse(string text, out CompactValue value) => TryParse(text.AsSpan(), out value);
        public static bool TryParse(ReadOnlySpan<char> text, out CompactValue value)
        {
            value = default;
            if (!UtilityNumber.TryParseMantissa(text, out double mantissa, out int exponent)) return false;
            value = new CompactValue(mantissa, exponent);
            return true;
        }
        public static CompactValue Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Zero;
            if (TryParse(text, out var value)) return value;
            throw new FormatException("Invalid compact number or exponent outside Int32 range.");
        }
        public bool TryFormat(Span<char> destination, out int charsWritten, int decimalDigits = 2)
            => UtilityNumber.TryFormatMantissa(Mantissa, Exponent, destination, out charsWritten, decimalDigits);
        public string ToCompactString(int decimalDigits = 2) => UtilityNumber.FormatMantissa(Mantissa, Exponent, decimalDigits);
        /// <summary>Lossless round trip for this representation; display text is rounded and must not be used for saving.</summary>
        public string ToSaveString() => Mantissa.ToString("R", System.Globalization.CultureInfo.InvariantCulture)
            + ":" + Exponent.ToString(System.Globalization.CultureInfo.InvariantCulture);
        public static bool TryParseSave(ReadOnlySpan<char> text, out CompactValue value)
        {
            value = default;
            int separator = text.IndexOf(':');
            if (separator < 0) return false;
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            if (!double.TryParse(text.Slice(0, separator), System.Globalization.NumberStyles.Float, culture, out double mantissa)
                || !int.TryParse(text.Slice(separator + 1), System.Globalization.NumberStyles.Integer, culture, out int exponent)
                || double.IsNaN(mantissa) || double.IsInfinity(mantissa)) return false;
            // Saved state is normalized, so load cannot overflow through normalization.
            if (mantissa == 0 ? exponent != 0 : Math.Abs(mantissa) < 1 || Math.Abs(mantissa) >= 1000) return false;
            value = new CompactValue(mantissa, exponent);
            return true;
        }
        public override string ToString() => ToCompactString();
    }
}
