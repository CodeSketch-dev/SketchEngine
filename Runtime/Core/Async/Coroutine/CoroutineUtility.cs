using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SketchEngine.Core.Async
{
    /// <summary>
    /// Tiện ích coroutine tĩnh, không cần truy cập MonoBehaviour nào.
    /// Các yield instruction không đổi được cache để không tạo rác mỗi lần gọi.
    /// </summary>
    public static class CoroutineUtility
    {
        // Giới hạn số lượng cache theo thời lượng, tránh phình bộ nhớ nếu game dùng quá nhiều giá trị khác nhau.
        const int MaxCachedDurations = 64;

        static readonly UnityEngine.WaitForEndOfFrame s_endOfFrame = new UnityEngine.WaitForEndOfFrame();
        static readonly UnityEngine.WaitForFixedUpdate s_fixedUpdate = new UnityEngine.WaitForFixedUpdate();
        static readonly Dictionary<int, UnityEngine.WaitForSeconds> s_scaledCache = new Dictionary<int, UnityEngine.WaitForSeconds>();
        static readonly Dictionary<int, UnityEngine.WaitForSecondsRealtime> s_realtimeCache = new Dictionary<int, UnityEngine.WaitForSecondsRealtime>();

        // =====================================================
        // BASIC COROUTINE
        // =====================================================

        public static Coroutine Start(IEnumerator routine)
        {
            return CoroutineGlobal.Run(routine);
        }

        public static void Stop(Coroutine coroutine)
        {
            CoroutineGlobal.Stop(coroutine);
        }

        // =====================================================
        // YIELD INSTRUCTION HELPERS
        // =====================================================

        public static void WaitForEndOfFrame(Action action)
        {
            CoroutineGlobal.RunAfter(s_endOfFrame, action);
        }

        public static void WaitForFixedUpdate(Action action)
        {
            CoroutineGlobal.RunAfter(s_fixedUpdate, action);
        }

        public static void WaitForSeconds(float seconds, Action action)
        {
            CoroutineGlobal.RunAfter(GetScaled(seconds), action);
        }

        public static void WaitForSecondsRealtime(float seconds, Action action)
        {
            CoroutineGlobal.RunAfter(GetRealtime(seconds), action);
        }

        public static void WaitForSecondsRandom(float min, float max, Action action)
        {
            WaitForSeconds(UnityEngine.Random.Range(min, max), action);
        }

        public static void WaitUntil(Func<bool> predicate, Action action)
        {
            if (predicate == null || action == null)
                return;

            // WaitUntil có trạng thái theo predicate nên không cache được.
            CoroutineGlobal.RunAfter(new UnityEngine.WaitUntil(predicate), action);
        }

        // =====================================================
        // CACHE
        // =====================================================

        static UnityEngine.WaitForSeconds GetScaled(float seconds)
        {
            int key = KeyOf(seconds);

            if (s_scaledCache.TryGetValue(key, out var cached))
                return cached;

            var instruction = new UnityEngine.WaitForSeconds(seconds);
            if (s_scaledCache.Count < MaxCachedDurations)
                s_scaledCache[key] = instruction;

            return instruction;
        }

        static UnityEngine.WaitForSecondsRealtime GetRealtime(float seconds)
        {
            int key = KeyOf(seconds);

            if (s_realtimeCache.TryGetValue(key, out var cached))
                return cached;

            var instruction = new UnityEngine.WaitForSecondsRealtime(seconds);
            if (s_realtimeCache.Count < MaxCachedDurations)
                s_realtimeCache[key] = instruction;

            return instruction;
        }

        // Khóa theo mili-giây: đủ chính xác cho coroutine, và tránh so sánh float.
        static int KeyOf(float seconds)
        {
            return Mathf.RoundToInt(Mathf.Max(0f, seconds) * 1000f);
        }
    }
}
