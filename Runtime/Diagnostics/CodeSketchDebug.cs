using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SketchEngine.Diagnostics
{
    /// <summary>
    /// Class that contains methods useful for debugging.
    /// All methods are only compiled if the DEVELOPMENT_BUILD symbol or UNITY_EDITOR is defined.
    /// </summary>
    public static class CodeSketchDebug
    {
        public static void LogRuntime(object message, Color? color = null)
        {
            Debug.Log(GetLog(message, color.GetValueOrDefault(Color.white)));
        }
        
        public static void LogRuntime(object header, object message, Color? headerColor = null)
        {
            Debug.Log(GetLog(header, message, headerColor.GetValueOrDefault(Color.white)));
        }
        
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object message, Color? color = null)
        {
            Debug.Log(GetLog(message, color.GetValueOrDefault(Color.white)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log(object header, object message, Color? headerColor = null)
        {
            Debug.Log(GetLog(header, message, headerColor.GetValueOrDefault(Color.white)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Log<T>(object message, Color? headerColor = null)
        {
            Debug.Log(GetLog(typeof(T), message, headerColor.GetValueOrDefault(Color.white)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(object message, Color? color = null)
        {
            Debug.LogWarning(GetLog(message, color.GetValueOrDefault(Color.white)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning(object header, object message, Color? headerColor = null)
        {
            Debug.LogWarning(GetLog(header, message, headerColor.GetValueOrDefault(Color.white)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarning<T>(object message, Color? headerColor = null)
        {
            Debug.LogWarning(GetLog(typeof(T), message, headerColor.GetValueOrDefault(Color.white)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(object message, Color? color = null)
        {
            Debug.LogError(GetLog(message, color.GetValueOrDefault(Color.white)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogError(object header, object message, Color? headerColor = null)
        {
            Debug.LogError(GetLog(header, message, headerColor.GetValueOrDefault(Color.white)));
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogError<T>(object message, Color? headerColor = null)
        {
            Debug.LogError(GetLog(typeof(T), message, headerColor.GetValueOrDefault(Color.white)));
        }

        static object GetLog(object message, Color color)
        {
            if (color == Color.white)
                return message;
            else
                return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>";
        }

        static object GetLog(object header, object message, Color color)
        {
            if (color == Color.white)
                return $"[{header}] {message}";
            else
                return $"[<color=#{ColorUtility.ToHtmlStringRGB(color)}>{header}</color>] {message}";
        }

        #region Log Caller (Log line code and file)

        public static void LogCallerAlways(
            Color? color = null,
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            var log = GetCallerLog(line, memberName, filePath, color.GetValueOrDefault(Color.white));
            Debug.Log(log);
        }
        
        public static void LogWarningCallerAlways(
            Color? color = null,
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            var log = GetCallerLog(line, memberName, filePath, color.GetValueOrDefault(Color.white));
            Debug.LogWarning(log);
        }
        
        public static void LogErrorCallerAlways(
            Color? color = null,
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            var log = GetCallerLog(line, memberName, filePath, color.GetValueOrDefault(Color.white));
            Debug.LogError(log);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogCaller(
            Color? color = null,
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            var log = GetCallerLog(line, memberName, filePath, color.GetValueOrDefault(Color.white));
            Debug.Log(log);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogWarningCaller(
            Color? color = null,
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            var log = GetCallerLog(line, memberName, filePath, color.GetValueOrDefault(Color.white));
            Debug.LogWarning(log);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void LogErrorCaller(
            Color? color = null,
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            var log = GetCallerLog(line, memberName, filePath, color.GetValueOrDefault(Color.white));
            Debug.LogError(log);
        }

        static string GetCallerLog(int line, string memberName, string filePath, Color color)
        {
            string fileName = Path.GetFileName(filePath);
            string colorHex = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{colorHex}>[Caller] {fileName}:{line} :: {memberName}</color>";
        }

        #endregion

    }
}
