using System;
using UnityEngine;

namespace SketchEngine.Utilities.Utils
{
    /// <summary>
    /// Unified time utilities for save/load, cooldown, offline progress,
    /// expiration, scheduling.
    /// Uses UTC Unix Timestamp (milliseconds).
    /// </summary>
    public static class UtilsTime
    {
        // =====================================================
        // NOW
        // =====================================================

        public static long NowMs => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public static long NowSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // =====================================================
        // CONVERT (ADDED)
        // =====================================================

        public static long ToUnixSeconds(DateTime utcDateTime)
            => new DateTimeOffset(utcDateTime).ToUnixTimeSeconds();

        public static long ToUnixMilliseconds(DateTime utcDateTime)
            => new DateTimeOffset(utcDateTime).ToUnixTimeMilliseconds();

        public static DateTime FromUnixSeconds(long unixSeconds)
            => DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;

        public static DateTime FromUnixMilliseconds(long unixMilliseconds)
            => DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds).UtcDateTime;

        // =====================================================
        // SAVE TIME
        // =====================================================

        public static long CreateSaveTime() => NowMs;
        public static bool IsValidSaveTime(long savedUnixMs) => savedUnixMs > 0;

        // =====================================================
        // DURATION
        // =====================================================

        public static int SecondsSince(long savedUnixMs)
        {
            if (savedUnixMs <= 0) return 0;
            long delta = NowMs - savedUnixMs;
            return delta <= 0 ? 0 : (int)(delta / 1000);
        }

        public static TimeSpan TimeSince(long savedUnixMs)
        {
            if (savedUnixMs <= 0) return TimeSpan.Zero;
            long delta = NowMs - savedUnixMs;
            return delta <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(delta);
        }

        public static int DurationSeconds(long startUnixMs, long endUnixMs)
        {
            long delta = endUnixMs - startUnixMs;
            return delta <= 0 ? 0 : (int)(delta / 1000);
        }

        // =====================================================
        // EXPIRE / COOLDOWN
        // =====================================================

        public static long CreateExpireAfterSeconds(int seconds)
        {
            return NowMs + (long)seconds * 1000;
        }

        public static bool IsExpired(long expireUnixMs)
        {
            return NowMs >= expireUnixMs;
        }

        public static int SecondsLeft(long expireUnixMs)
        {
            long delta = expireUnixMs - NowMs;
            return delta <= 0 ? 0 : (int)(delta / 1000);
        }

        public static TimeSpan TimeLeft(long expireUnixMs)
        {
            long delta = expireUnixMs - NowMs;
            return delta <= 0 ? TimeSpan.Zero : TimeSpan.FromMilliseconds(delta);
        }

        // =====================================================
        // RANGE / CLAMP
        // =====================================================

        public static bool IsNowInRange(long startUnixMs, long endUnixMs)
        {
            long now = NowMs;
            return now >= startUnixMs && now <= endUnixMs;
        }

        public static int ClampElapsedSeconds(long savedUnixMs, int maxSeconds)
        {
            return Mathf.Min(SecondsSince(savedUnixMs), maxSeconds);
        }

        public static long ClampUnix(long unixMs, long min, long max)
        {
            if (unixMs < min) return min;
            if (unixMs > max) return max;
            return unixMs;
        }

        // =====================================================
        // PROGRESS (ADDED)
        // =====================================================

        /// <summary>
        /// Progress from start → end, clamped 0–1
        /// </summary>
        public static float Progress01(long startUnixMs, long endUnixMs)
        {
            if (endUnixMs <= startUnixMs) return 1f;
            return Mathf.Clamp01((float)(NowMs - startUnixMs) / (endUnixMs - startUnixMs));
        }

        // =====================================================
        // FORMAT – CORE
        // =====================================================

        /// <summary>
        /// Format TimeSpan as:
        /// HH:mm:ss (>= 1 hour)
        /// mm:ss    (< 1 hour)
        /// </summary>
        public static string FormatTime(TimeSpan time)
        {
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            else
                return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        // =====================================================
        // FORMAT – FROM SECONDS
        // =====================================================

        public static string FormatMMSS(int seconds)
        {
            if (seconds <= 0) return "00:00";
            int m = seconds / 60;
            int s = seconds % 60;
            return $"{m:D2}:{s:D2}";
        }

        public static string FormatHHMMSS(int seconds)
        {
            if (seconds <= 0) return "00:00:00";
            int h = seconds / 3600;
            int m = (seconds % 3600) / 60;
            int s = seconds % 60;
            return $"{h:D2}:{m:D2}:{s:D2}";
        }

        public static string FormatAuto(int seconds)
        {
            return seconds >= 3600
                ? FormatHHMMSS(seconds)
                : FormatMMSS(seconds);
        }

        // =====================================================
        // FORMAT – FROM TIMESTAMP
        // =====================================================

        public static string FormatTimeLeft(long expireUnixMs)
        {
            return FormatAuto(SecondsLeft(expireUnixMs));
        }

        public static string FormatTimeSince(long savedUnixMs)
        {
            return FormatAuto(SecondsSince(savedUnixMs));
        }

        // =====================================================
        // FORMAT – HUMAN READABLE
        // =====================================================

        public static string FormatHuman(int seconds)
        {
            if (seconds <= 0) return "0s";

            int d = seconds / 86400;
            seconds %= 86400;
            int h = seconds / 3600;
            seconds %= 3600;
            int m = seconds / 60;
            int s = seconds % 60;

            if (d > 0) return $"{d}d {h}h {m}m";
            if (h > 0) return $"{h}h {m}m";
            if (m > 0) return $"{m}m {s}s";
            return $"{s}s";
        }
    }
}
