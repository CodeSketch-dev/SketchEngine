using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SketchEngine.Utilities.Utils
{
    /// <summary>
    /// Text utilities optimized for mobile UI (TMP).
    /// Color tags, keyword highlight, time formatting.
    /// </summary>
    public static class UtilsText
    {
        // =====================================================
        // COLORIZE BY INDEX (SAFE)
        // =====================================================

        /// <summary>
        /// Colorize text by index segments.
        /// Indexes are clamped and sorted automatically.
        /// </summary>
        public static string ColorizeText(
            string text,
            List<(int start, int length, Color color)> segments)
        {
            if (string.IsNullOrEmpty(text) || segments == null || segments.Count == 0)
                return text;

            segments.Sort((a, b) => a.start.CompareTo(b.start));

            var sb = new StringBuilder(text.Length + segments.Count * 16);
            int cursor = 0;

            foreach (var (start, length, color) in segments)
            {
                int s = Mathf.Clamp(start, 0, text.Length);
                int e = Mathf.Clamp(start + length, s, text.Length);

                if (cursor < s)
                    sb.Append(text, cursor, s - cursor);

                sb.Append("<color=#");
                sb.Append(ColorUtility.ToHtmlStringRGB(color));
                sb.Append(">");

                sb.Append(text, s, e - s);

                sb.Append("</color>");

                cursor = e;
            }

            if (cursor < text.Length)
                sb.Append(text, cursor, text.Length - cursor);

            return sb.ToString();
        }

        // =====================================================
        // SIMPLE COLORIZE
        // =====================================================

        public static string ColorizeText(string msg, Color? color = null)
        {
            Color c = color ?? Color.white;
            return $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{msg}</color>";
        }

        public static string ColorizeText(int msg, Color? color = null)
        {
            Color c = color ?? Color.white;
            return $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{msg}</color>";
        }

        // =====================================================
        // COLORIZE BY KEYWORD (LOW GC)
        // =====================================================

        /// <summary>
        /// Highlight keywords inside text.
        /// Avoids repeated ToString() allocations.
        /// </summary>
        public static string ColorizeByKeyword(
            string text,
            List<(string keyword, Color color)> highlights,
            bool onlyFirstMatch = true)
        {
            if (string.IsNullOrEmpty(text) || highlights == null || highlights.Count == 0)
                return text;

            var sb = new StringBuilder(text);

            foreach (var (keyword, color) in highlights)
            {
                if (string.IsNullOrEmpty(keyword))
                    continue;

                string openTag = "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">";
                const string closeTag = "</color>";

                int index = sb.ToString().IndexOf(keyword, StringComparison.Ordinal);
                if (index < 0)
                    continue;

                sb.Insert(index + keyword.Length, closeTag);
                sb.Insert(index, openTag);

                if (onlyFirstMatch)
                    break;
            }

            return sb.ToString();
        }

        // =====================================================
        // TIME FORMAT (UI)
        // =====================================================

        /// <summary>
        /// Format seconds to mm:ss
        /// </summary>
        public static string ToClockFormatMMSS(float seconds)
        {
            int m = (int)(seconds / 60f);
            int s = (int)(seconds % 60f);
            return $"{m:00}:{s:00}";
        }

        /// <summary>
        /// Format seconds to HH:mm:ss
        /// </summary>
        public static string ToClockFormatHHMMSS(float seconds)
        {
            int h = (int)(seconds / 3600f);
            int m = (int)((seconds % 3600f) / 60f);
            int s = (int)(seconds % 60f);
            return $"{h:00}:{m:00}:{s:00}";
        }

        // =====================================================
        // BONUS: SAFE TMP COUNTER FORMAT
        // =====================================================

        /// <summary>
        /// Update TMP text only if value changed (reduce GC).
        /// </summary>
        public static bool SetTextIfChanged(ref string cache, string newValue, out string result)
        {
            if (cache == newValue)
            {
                result = cache;
                return false;
            }

            cache = newValue;
            result = newValue;
            return true;
        }
    }
}
