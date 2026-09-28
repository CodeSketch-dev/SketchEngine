using System;
using System.Collections;
using UnityEngine;

namespace SketchEngine.Core.Async
{
    /// <summary>
    /// Static coroutine helper utilities.
    /// 
    /// This class provides a safe, static facade over the global coroutine runner
    /// (<see cref="CoroutineGlobal"/>), allowing coroutine usage without directly
    /// accessing any <see cref="MonoBehaviour"/>.
    /// </summary>
    public static class CoroutineUtility
    {
        // =====================================================
        // BASIC COROUTINE
        // =====================================================

        /// <summary>
        /// Starts a coroutine using the global coroutine runner.
        /// Equivalent to MonoBehaviour.StartCoroutine.
        /// </summary>
        /// <param name="routine">Coroutine routine.</param>
        /// <returns>Started Coroutine.</returns>
        public static Coroutine Start(IEnumerator routine)
        {
            return CoroutineGlobal.Run(routine);
        }

        /// <summary>
        /// Stops a coroutine started via <see cref="Start"/>.
        /// </summary>
        /// <param name="coroutine">Coroutine reference.</param>
        public static void Stop(Coroutine coroutine)
        {
            CoroutineGlobal.Stop(coroutine);
        }

        // =====================================================
        // YIELD INSTRUCTION HELPERS
        // =====================================================

        /// <summary>
        /// Executes an action at the end of the current frame.
        /// </summary>
        public static void WaitForEndOfFrame(Action action)
        {
            CoroutineGlobal.RunAfter(new WaitForEndOfFrame(), action);
        }

        /// <summary>
        /// Executes an action on the next FixedUpdate tick.
        /// </summary>
        public static void WaitForFixedUpdate(Action action)
        {
            CoroutineGlobal.RunAfter(new WaitForFixedUpdate(), action);
        }

        /// <summary>
        /// Executes an action after a delay (scaled time).
        /// </summary>
        /// <param name="seconds">Delay in seconds.</param>
        public static void WaitForSeconds(float seconds, Action action)
        {
            CoroutineGlobal.RunAfter(new WaitForSeconds(seconds), action);
        }

        /// <summary>
        /// Executes an action after a delay (unscaled realtime).
        /// </summary>
        /// <param name="seconds">Delay in seconds.</param>
        public static void WaitForSecondsRealtime(float seconds, Action action)
        {
            CoroutineGlobal.RunAfter(new WaitForSecondsRealtime(seconds), action);
        }

        /// <summary>
        /// Executes an action after a random delay between min and max (scaled time).
        /// </summary>
        public static void WaitForSecondsRandom(float min, float max, Action action)
        {
            float delay = UnityEngine.Random.Range(min, max);
            WaitForSeconds(delay, action);
        }
        
        /// <summary>
        /// Executes an action when the given condition becomes true.
        /// 
        /// Example:
        /// CoroutineUtility.WaitUntil(() => isReady, OnReady);
        /// </summary>
        /// <param name="predicate">Condition to evaluate.</param>
        /// <param name="action">Callback when condition is met.</param>
        public static void WaitUntil(Func<bool> predicate, Action action)
        {
            if (predicate == null || action == null)
                return;

            CoroutineGlobal.RunAfter(new WaitUntil(predicate), action);
        }
    }
}
