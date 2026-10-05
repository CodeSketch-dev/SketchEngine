using System;
using System.Collections.Concurrent;
using SketchEngine.Mono;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SketchEngine.Core.MainThread
{
    /// <summary>
    /// Đưa action từ thread nền về main thread. Có thể gọi Enqueue từ bất kỳ thread nào.
    /// Mỗi frame xử lý trong một ngân sách thời gian nhỏ để không gây giật frame.
    /// Action còn lại không bị bỏ, sẽ chạy ở các frame sau, đúng thứ tự enqueue.
    /// </summary>
    public static class MainThreadDispatcher
    {
        // Ngân sách xử lý mỗi frame (ms). Kiểm tra thời gian mỗi CheckTimeEvery action để giảm chi phí đọc đồng hồ.
        const float FrameBudgetMs = 1f;
        const int CheckTimeEvery = 8;

        static readonly ConcurrentQueue<Action> s_queue = new ConcurrentQueue<Action>();
        static bool s_subscribed;

        /// <summary>Thêm action vào hàng đợi, sẽ chạy trên main thread ở frame sau. An toàn từ mọi thread.</summary>
        public static void Enqueue(Action action)
        {
            if (action == null) return;
            s_queue.Enqueue(action);
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void SubscribeEditor()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }
#else
        // AfterSceneLoad: MonoCallback cần scene và main thread. Enqueue trước đó vẫn an toàn, chỉ chờ.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SubscribeRuntime()
        {
            if (s_subscribed) return;
            s_subscribed = true;

            MonoCallback.SafeInstance.EventUpdate += Tick;
        }
#endif

        // Xử lý trong ngân sách thời gian mỗi frame. Phần còn lại không bị bỏ: vẫn nằm trong hàng đợi và chạy ở frame sau, đúng thứ tự.
        static void Tick()
        {
            if (s_queue.IsEmpty) return;

            float start = UnityEngine.Time.realtimeSinceStartup;
            int processed = 0;

            while (s_queue.TryDequeue(out Action action))
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    // Một action lỗi không được làm hỏng các action còn lại.
                    Debug.LogException(e);
                }

                processed++;
                if (processed % CheckTimeEvery == 0 &&
                    (UnityEngine.Time.realtimeSinceStartup - start) * 1000f >= FrameBudgetMs)
                    break;
            }
        }
    }
}
