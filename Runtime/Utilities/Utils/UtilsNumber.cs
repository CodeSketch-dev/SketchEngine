using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using SketchEngine.Diagnostics;

namespace SketchEngine.Utilities.Utils
{
    /// <summary>
    /// Compact number formatting & parsing utilities.
    /// Supports infinite units: K M B T a b c ... z aa ab ... zz aaa ...
    /// Case-insensitive parsing, stable for idle games.
    /// </summary>
    public static class UtilsNumber
    {
        // =====================================================
        // CONST / CACHE
        // =====================================================

        const double MinValueFormat = 999.0;

        // Base units (fixed)
        static readonly string[] BaseUnits =
        {
            "", "K", "M", "B", "T"
        };

        static readonly CultureInfo CultureInvariant = CultureInfo.InvariantCulture;

        static readonly CultureInfo CultureVN_Dot = new CultureInfo("vi-VN")
        {
            NumberFormat =
            {
                NumberGroupSeparator = ".",
                NumberDecimalSeparator = "."
            }
        };

        static readonly CultureInfo CultureVN_Comma = new CultureInfo("vi-VN")
        {
            NumberFormat =
            {
                NumberGroupSeparator = ".",
                NumberDecimalSeparator = ","
            }
        };

        // =====================================================
        // FORMAT
        // =====================================================

        public static string Format(float value) => Format((double)value);

        public static string Format(double value, int decimalDigits = 2)
        {
            if (Math.Abs(value) < MinValueFormat)
                return ((long)value).ToString(CultureInvariant);

            int sign = value < 0 ? -1 : 1;
            double magnitude = Math.Abs(value);

            int unitIndex = 0;
            while (magnitude >= 1000.0)
            {
                magnitude /= 1000.0;
                unitIndex++;
            }

            string unit = unitIndex < BaseUnits.Length
                ? BaseUnits[unitIndex]
                : ToExtendedUnit(unitIndex - BaseUnits.Length);

            return (magnitude * sign).ToString(
                       DecimalFormat(decimalDigits),
                       CultureInvariant
                   ) + unit;
        }

        // =====================================================
        // FORMAT VN
        // =====================================================

        public static string FormatVN(double value, int decimalDigits = 2)
        {
            if (Math.Abs(value) < MinValueFormat)
            {
                string format = DecimalFormat(decimalDigits);
                return Math.Round(value, decimalDigits).ToString(format, CultureVN_Dot);
            }

            int sign = value < 0 ? -1 : 1;
            double magnitude = Math.Abs(value);

            int unitIndex = 0;
            while (magnitude >= 1000.0)
            {
                magnitude /= 1000.0;
                unitIndex++;
            }

            string unit = unitIndex < BaseUnits.Length
                ? BaseUnits[unitIndex]
                : ToExtendedUnit(unitIndex - BaseUnits.Length);

            // Dùng placeholder '#' thay vì "F{digits}" + TrimEnd('0'): TrimEnd ăn luôn số 0
            // ở cuối phần nguyên khi không có dấu thập phân (vd decimalDigits=0: "400" -> "4").
            return (magnitude * sign).ToString(DecimalFormat(decimalDigits), CultureInvariant) + unit;
        }

        public static string FormatVN_CompactRounded(double value, int decimalDigits = 2)
        {
            if (Math.Abs(value) < MinValueFormat)
                return Math.Round(value).ToString("N0", CultureVN_Dot);

            int sign = value < 0 ? -1 : 1;
            double magnitude = Math.Abs(value);

            int unitIndex = 0;
            while (magnitude >= 1000.0)
            {
                magnitude /= 1000.0;
                unitIndex++;
            }

            string unit = unitIndex < BaseUnits.Length
                ? BaseUnits[unitIndex]
                : ToExtendedUnit(unitIndex - BaseUnits.Length);

            // Cùng lý do như FormatVN: placeholder '#' giữ nguyên số 0 ở phần nguyên,
            // chỉ bỏ số 0 thừa ở phần thập phân.
            return (magnitude * sign).ToString(DecimalFormat(decimalDigits), CultureInvariant) + unit;
        }

        // Format string dùng '0' cho phần nguyên (luôn giữ) và '#' cho phần thập phân
        // (tự bỏ số 0 thừa, không đụng tới phần nguyên) -> thay thế an toàn cho "F{digits}"+TrimEnd.
        static string DecimalFormat(int decimalDigits)
        {
            if (decimalDigits < 0) decimalDigits = 0;
            return decimalDigits > 0
                ? "0." + new string('#', decimalDigits)
                : "0";
        }

        // =====================================================
        // PARSE
        // =====================================================

        public static float ParseCompactNumberFloat(string val)
            => (float)ParseCompactNumber(val);

        public static double ParseCompactNumber(string val)
        {
            if (string.IsNullOrWhiteSpace(val))
                return 0;

            val = val.Trim();

            int letterIndex = -1;
            for (int i = 0; i < val.Length; i++)
            {
                if (char.IsLetter(val[i]))
                {
                    letterIndex = i;
                    break;
                }
            }

            // No unit
            if (letterIndex == -1)
            {
                if (double.TryParse(val, NumberStyles.Float, CultureInvariant, out var plain))
                    return plain;

                CodeSketchDebug.Log($"ParseCompactNumber: Cannot parse '{val}'", Color.red);
                return 0;
            }

            string numberPart = val.Substring(0, letterIndex);
            string unitPart = val.Substring(letterIndex).Trim();

            if (!double.TryParse(numberPart, NumberStyles.Float, CultureInvariant, out var baseValue))
            {
                CodeSketchDebug.Log($"ParseCompactNumber: Cannot parse '{numberPart}'", Color.red);
                return 0;
            }

            double multiplier = GetUnitMultiplier(unitPart);
            return baseValue * multiplier;
        }

        static double GetUnitMultiplier(string unit)
        {
            if (string.IsNullOrEmpty(unit))
                return 1;

            unit = unit.Trim();

            // Single-letter units:
            // K M B T a b c ... z
            if (unit.Length == 1)
            {
                return char.ToUpperInvariant(unit[0]) switch
                {
                    'K' => 1_000d,
                    'M' => 1_000_000d,
                    'B' => 1_000_000_000d,
                    'T' => 1_000_000_000_000d,
                    _ when unit[0] >= 'a' && unit[0] <= 'z' => Math.Pow(1000d, 5 + (unit[0] - 'a')),
                    _ when unit[0] >= 'A' && unit[0] <= 'Z' => Math.Pow(1000d, 5 + (unit[0] - 'A')),
                    _ => 1
                };
            }

            // Extended units:
            // aa, ab, ... zz, aaa ...
            int extendedIndex = FromExtendedUnit(unit.ToLowerInvariant());
            if (extendedIndex < 0)
                return 1;

            return Math.Pow(1000d, 5 + extendedIndex);
        }

        // =====================================================
        // EXTENDED UNIT (a, b, ... z, aa, ab, ... aaa ...)
        // =====================================================

        static string ToExtendedUnit(int index)
        {
            // 0 -> a, 1 -> b, ... 25 -> z, 26 -> aa
            string result = "";
            do
            {
                result = (char)('a' + (index % 26)) + result;
                index = index / 26 - 1;
            }
            while (index >= 0);
            return result;
        }

        static int FromExtendedUnit(string unit)
        {
            int result = 0;

            foreach (char c in unit)
            {
                int v = c - 'a';
                if (v < 0 || v > 25)
                    return -1;

                result = result * 26 + v + 1;
            }

            return result - 1;
        }

        // =====================================================
        // COMMON PRICE / SEPARATOR
        // =====================================================

        public static string FormatPriceVN(int value)
            => value.ToString("N0", CultureVN_Dot);

        public static string FormatWithSeparator(double value)
            => value.ToString("N0", CultureVN_Comma);

        public static string FormatWithSeparator(double value, int decimalDigits)
            => value.ToString("N" + decimalDigits, CultureVN_Comma);

        public static string FormatWithSeparator(float value)
            => FormatWithSeparator((double)value);
    }

    // =========================================================
    // COMPACT NUMBER MATH (SAFE MONEY OPS)
    // =========================================================

    public static class CompactNumberMath
    {
        public static string Add(string a, string b)
            => UtilsNumber.Format(
                UtilsNumber.ParseCompactNumber(a) +
                UtilsNumber.ParseCompactNumber(b)
            );

        public static string Subtract(string a, string b)
            => UtilsNumber.Format(
                Math.Max(
                    UtilsNumber.ParseCompactNumber(a) -
                    UtilsNumber.ParseCompactNumber(b),
                    0
                )
            );

        public static string Multiply(string a, float multiplier)
            => UtilsNumber.Format(
                UtilsNumber.ParseCompactNumber(a) * multiplier
            );

        public static string Divide(string a, float divisor)
        {
            if (divisor <= 0)
                return "0";

            return UtilsNumber.Format(
                UtilsNumber.ParseCompactNumber(a) / divisor
            );
        }

        public static bool GreaterThan(string a, string b)
            => UtilsNumber.ParseCompactNumber(a) >
               UtilsNumber.ParseCompactNumber(b);
    }
}
