using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SketchEngine.Utilities.Utils
{
    /// <summary>
    /// Mobile-friendly text helpers for UI/TMP: rich-text coloring, keyword highlight,
    /// clock formatting and tag stripping. Methods reuse one shared StringBuilder and
    /// match buffer to avoid per-call GC; intended for Unity's main thread only.
    /// </summary>
    public static class UtilityText
    {
        const string ColorTagOpen = "<color=#";
        const string ColorTagClose = ">";
        const string ColorTagEnd = "</color>";
        static readonly char[] HexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        static StringBuilder _builder;
        static readonly List<(int start, int end, Color32 color)> MatchBuffer = new List<(int, int, Color32)>(8);

        /// <summary>Shared scratch builder. Not reentrant: do not hold the result across another call before copying it out via ToString().</summary>
        static StringBuilder RentBuilder(int capacityHint)
        {
            if (_builder == null) _builder = new StringBuilder(Mathf.Max(capacityHint, 64));
            else _builder.Length = 0;
            if (_builder.Capacity < capacityHint) _builder.Capacity = capacityHint;
            return _builder;
        }

        // =====================================================
        // HEX COLOR (NO GC)
        // =====================================================

        /// <summary>Appends "RRGGBB" without allocating a string, unlike ColorUtility.ToHtmlStringRGB.</summary>
        public static void AppendHexRGB(StringBuilder sb, Color32 color)
        {
            AppendHexByte(sb, color.r);
            AppendHexByte(sb, color.g);
            AppendHexByte(sb, color.b);
        }

        static void AppendHexByte(StringBuilder sb, byte value)
        {
            sb.Append(HexDigits[value >> 4]);
            sb.Append(HexDigits[value & 0xF]);
        }

        /// <summary>Writes "RRGGBB" into caller-owned storage. False means destination is shorter than 6 chars.</summary>
        public static bool TryFormatHexRGB(Color32 color, Span<char> destination, out int charsWritten)
        {
            if (destination.Length < 6) { charsWritten = 0; return false; }
            WriteHexByte(color.r, destination);
            WriteHexByte(color.g, destination.Slice(2));
            WriteHexByte(color.b, destination.Slice(4));
            charsWritten = 6;
            return true;
        }

        static void WriteHexByte(byte value, Span<char> destination)
        {
            destination[0] = HexDigits[value >> 4];
            destination[1] = HexDigits[value & 0xF];
        }

        static void AppendColorOpenTag(StringBuilder sb, Color32 color)
        {
            sb.Append(ColorTagOpen);
            AppendHexRGB(sb, color);
            sb.Append(ColorTagClose);
        }

        // =====================================================
        // COLORIZE BY INDEX SEGMENTS
        // =====================================================

        /// <summary>
        /// Wraps index ranges of <paramref name="text"/> in color tags. Segments are sorted
        /// in place and clamped to the text bounds; overlapping ranges are truncated, not merged.
        /// </summary>
        public static string ColorizeSegments(string text, List<(int start, int length, Color32 color)> segments)
        {
            if (string.IsNullOrEmpty(text) || segments == null || segments.Count == 0)
                return text;

            segments.Sort((a, b) => a.start.CompareTo(b.start));

            var sb = RentBuilder(text.Length + segments.Count * 16);
            int cursor = 0;

            foreach (var (start, length, color) in segments)
            {
                int s = Mathf.Clamp(start, cursor, text.Length);
                int e = Mathf.Clamp(start + length, s, text.Length);
                if (s >= e) continue;

                if (cursor < s) sb.Append(text, cursor, s - cursor);
                AppendColorOpenTag(sb, color);
                sb.Append(text, s, e - s);
                sb.Append(ColorTagEnd);
                cursor = e;
            }

            if (cursor < text.Length) sb.Append(text, cursor, text.Length - cursor);
            return sb.ToString();
        }

        /// <summary>Wraps the whole string in a color tag.</summary>
        public static string Colorize(string text, Color32 color)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var sb = RentBuilder(text.Length + 24);
            AppendColorOpenTag(sb, color);
            sb.Append(text);
            sb.Append(ColorTagEnd);
            return sb.ToString();
        }

        /// <summary>Wraps a formatted integer in a color tag; avoids boxing via StringBuilder.Append(int).</summary>
        public static string Colorize(int value, Color32 color)
        {
            var sb = RentBuilder(32);
            AppendColorOpenTag(sb, color);
            sb.Append(value);
            sb.Append(ColorTagEnd);
            return sb.ToString();
        }

        // =====================================================
        // COLORIZE BY KEYWORD
        // =====================================================

        /// <summary>
        /// Highlights each keyword's first occurrence in <paramref name="text"/>. Matches are resolved
        /// against the original string (not the growing buffer), so searches never re-scan an already
        /// inserted tag. When <paramref name="onlyFirstMatch"/> is true, only the first keyword that
        /// matches is highlighted.
        /// </summary>
        public static string ColorizeKeywords(string text, IReadOnlyList<(string keyword, Color32 color)> highlights, bool onlyFirstMatch = true)
        {
            if (string.IsNullOrEmpty(text) || highlights == null || highlights.Count == 0)
                return text;

            MatchBuffer.Clear();
            for (int i = 0; i < highlights.Count; i++)
            {
                var (keyword, color) = highlights[i];
                if (string.IsNullOrEmpty(keyword)) continue;

                int index = text.IndexOf(keyword, StringComparison.Ordinal);
                if (index < 0) continue;

                MatchBuffer.Add((index, index + keyword.Length, color));
                if (onlyFirstMatch) break;
            }

            if (MatchBuffer.Count == 0) return text;
            if (MatchBuffer.Count > 1) MatchBuffer.Sort((a, b) => a.start.CompareTo(b.start));

            var sb = RentBuilder(text.Length + MatchBuffer.Count * 16);
            int cursor = 0;
            foreach (var (start, end, color) in MatchBuffer)
            {
                if (start < cursor) continue; // overlapping match from an earlier keyword

                sb.Append(text, cursor, start - cursor);
                AppendColorOpenTag(sb, color);
                sb.Append(text, start, end - start);
                sb.Append(ColorTagEnd);
                cursor = end;
            }
            sb.Append(text, cursor, text.Length - cursor);
            return sb.ToString();
        }

        // =====================================================
        // RICH TEXT
        // =====================================================

        /// <summary>Removes "&lt;...&gt;" rich-text tags. Returns the original instance if nothing was stripped.</summary>
        public static string StripRichText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            int firstTag = text.IndexOf('<');
            if (firstTag < 0) return text;

            var sb = RentBuilder(text.Length);
            int depth = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '<') { depth++; continue; }
                if (c == '>') { if (depth > 0) depth--; continue; }
                if (depth == 0) sb.Append(c);
            }
            return sb.ToString();
        }

        // =====================================================
        // TRUNCATE
        // =====================================================

        /// <summary>Truncates to at most <paramref name="maxLength"/> visible chars, appending the ellipsis. No allocation when already short enough.</summary>
        public static string TruncateWithEllipsis(string text, int maxLength, string ellipsis = "...")
        {
            if (string.IsNullOrEmpty(text) || maxLength <= 0 || text.Length <= maxLength)
                return text;

            int keep = Mathf.Max(0, maxLength - ellipsis.Length);
            var sb = RentBuilder(keep + ellipsis.Length);
            sb.Append(text, 0, keep);
            sb.Append(ellipsis);
            return sb.ToString();
        }

        // =====================================================
        // CLOCK FORMAT (NO GC)
        // =====================================================

        /// <summary>Formats seconds as mm:ss into caller-owned storage (5 chars). False means destination is too short.</summary>
        public static bool TryFormatClockMMSS(float seconds, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            if (destination.Length < 5) return false;

            int total = (int)Mathf.Max(seconds, 0f);
            int m = (total / 60) % 100;
            int s = total % 60;
            if (m > 99) m = 99;

            WriteTwoDigits(m, destination);
            destination[2] = ':';
            WriteTwoDigits(s, destination.Slice(3));
            charsWritten = 5;
            return true;
        }

        /// <summary>Formats seconds as HH:mm:ss into caller-owned storage (8 chars). False means destination is too short.</summary>
        public static bool TryFormatClockHHMMSS(float seconds, Span<char> destination, out int charsWritten)
        {
            charsWritten = 0;
            if (destination.Length < 8) return false;

            int total = (int)Mathf.Max(seconds, 0f);
            int h = (total / 3600) % 100;
            int m = (total % 3600) / 60;
            int s = total % 60;
            if (h > 99) h = 99;

            WriteTwoDigits(h, destination);
            destination[2] = ':';
            WriteTwoDigits(m, destination.Slice(3));
            destination[5] = ':';
            WriteTwoDigits(s, destination.Slice(6));
            charsWritten = 8;
            return true;
        }

        static void WriteTwoDigits(int value, Span<char> destination)
        {
            destination[0] = HexDigits[(value / 10) % 10];
            destination[1] = HexDigits[value % 10];
        }

        /// <summary>Allocates and returns "mm:ss". Prefer <see cref="TryFormatClockMMSS"/> on a hot path.</summary>
        public static string ToClockFormatMMSS(float seconds)
        {
            Span<char> buffer = stackalloc char[5];
            TryFormatClockMMSS(seconds, buffer, out int written);
            return new string(buffer.Slice(0, written));
        }

        /// <summary>Allocates and returns "HH:mm:ss". Prefer <see cref="TryFormatClockHHMMSS"/> on a hot path.</summary>
        public static string ToClockFormatHHMMSS(float seconds)
        {
            Span<char> buffer = stackalloc char[8];
            TryFormatClockHHMMSS(seconds, buffer, out int written);
            return new string(buffer.Slice(0, written));
        }

        // =====================================================
        // CHANGE-GUARDED ASSIGNMENT
        // =====================================================

        /// <summary>Updates <paramref name="cache"/> only when the value changed, so callers can skip a redundant TMP SetText.</summary>
        public static bool SetTextIfChanged(ref string cache, string newValue)
        {
            if (string.Equals(cache, newValue, StringComparison.Ordinal)) return false;
            cache = newValue;
            return true;
        }
    }
}
