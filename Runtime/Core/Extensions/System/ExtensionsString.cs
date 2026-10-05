using System.Runtime.CompilerServices;
using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    // Helper rich text cho TextMeshPro. Dùng với "text".SetColor(...).ToBold() ...
    public static class ExtensionsString
    {
        public static string RemoveQuotes(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            if (str.Length >= 2 && str[0] == '"' && str[str.Length - 1] == '"')
                return str.Substring(1, str.Length - 2);

            return str;
        }

        // hex không có dấu '#', vd "FF0000"
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SetColor(this string text, string hex) => $"<color=#{hex}>{text}</color>";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SetColor(this string text, Color color) => $"<color={ToHtmlStringRGBA(color)}>{text}</color>";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SetSize(this string text, int size) => $"<size={size}>{text}</size>";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBold(this string text) => $"<b>{text}</b>";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToItalic(this string text) => $"<i>{text}</i>";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToUnderline(this string text) => $"<u>{text}</u>";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStrikethrough(this string text) => $"<s>{text}</s>";

        // Color -> Color32 tự clamp về 0-255 trong Unity, không cần tự làm tròn.
        static string ToHtmlStringRGBA(Color color)
        {
            Color32 c = color;
            return $"#{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}";
        }
    }
}
