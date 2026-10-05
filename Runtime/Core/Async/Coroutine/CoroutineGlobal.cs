using System;
using System.Collections;
using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Core.Async
{
    /// <summary>
    /// Global coroutine runner.
    /// 
    /// Purpose:
    /// - Run YieldInstruction / CustomYieldInstruction with callback
    /// - Run IEnumerator coroutine with completion callback
    /// - Scene-safe (DontDestroyOnLoad)
    /// 
    /// Usage:
    /// - Call ONLY via static methods
    /// - Never access instance directly
    /// </summary>
    public sealed class CoroutineGlobal : MonoSingleton<CoroutineGlobal>
    {
        // =====================================================
        // CONFIG
        // =====================================================

        protected override bool PersistAcrossScenes => true;

        // =====================================================
        // PUBLIC STATIC API
        // =====================================================

        /// <summary>
        /// Execute an action after a YieldInstruction completes.
        /// 
        /// Example:
        /// CSKCoroutineGlobal.RunAfter(new WaitForSeconds(1f), () =>
        /// {
        ///     Debug.Log("1 second passed");
        /// });
        /// </summary>
        public static void RunAfter(YieldInstruction instruction, Action action)
        {
            if (instruction == null || action == null)
                return;

            SafeInstance.Internal_RunAfter(instruction, action);
        }

        /// <summary>
        /// Execute an action after a CustomYieldInstruction completes.
        /// 
        /// Example:
        /// CSKCoroutineGlobal.RunAfter(new WaitUntil(() => isReady), OnReady);
        /// </summary>
        public static void RunAfter(CustomYieldInstruction instruction, Action action)
        {
            if (instruction == null || action == null)
                return;

            SafeInstance.Internal_RunAfter(instruction, action);
        }

        /// <summary>
        /// Start a coroutine and invoke callback when it finishes.
        /// 
        /// Example:
        /// CSKCoroutineGlobal.Run(MyCoroutine(), () =>
        /// {
        ///     Debug.Log("Coroutine completed");
        /// });
        /// </summary>
        public static Coroutine Run(IEnumerator coroutine, Action onComplete = null)
        {
            if (coroutine == null)
                return null;

            return SafeInstance.Internal_RunCoroutine(coroutine, onComplete);
        }

        /// <summary>
        /// Stop a coroutine started by CSKCoroutineGlobal.Run().
        /// </summary>
        public static void Stop(Coroutine coroutine)
        {
            if (coroutine == null || !HasInstance)
                return;

            Instance.Internal_StopCoroutine(coroutine);
        }

        // =====================================================
        // INTERNAL IMPLEMENTATION
        // =====================================================

        void Internal_RunAfter(YieldInstruction instruction, Action action)
        {
            StartCoroutine(Co_RunAfter(instruction, action));
        }

        void Internal_RunAfter(CustomYieldInstruction instruction, Action action)
        {
            StartCoroutine(Co_RunAfter(instruction, action));
        }

        Coroutine Internal_RunCoroutine(IEnumerator coroutine, Action onComplete)
        {
            return StartCoroutine(Co_RunCoroutine(coroutine, onComplete));
        }

        void Internal_StopCoroutine(Coroutine coroutine)
        {
            StopCoroutine(coroutine);
        }

        // =====================================================
        // COROUTINES
        // =====================================================

        IEnumerator Co_RunAfter(YieldInstruction instruction, Action action)
        {
            yield return instruction;
            action.Invoke();
        }

        IEnumerator Co_RunAfter(CustomYieldInstruction instruction, Action action)
        {
            yield return instruction;
            action.Invoke();
        }

        // yield trực tiếp IEnumerator: Unity chạy nó lồng trong coroutine này, không cần StartCoroutine thêm.
        IEnumerator Co_RunCoroutine(IEnumerator coroutine, Action onComplete)
        {
            yield return coroutine;
            onComplete?.Invoke();
        }
    }
}
