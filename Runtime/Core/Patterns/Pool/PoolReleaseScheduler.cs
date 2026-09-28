using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SketchEngine.Patterns.Pool
{
    public static class PoolReleaseScheduler
    {
        const int ItemsPerFrame = 16;
        const int TaskPerFrame = 32;

        static readonly Queue<PoolPrefabItem> _queue = new();
        static readonly HashSet<PoolPrefabItem> _inQueue   = new();
        static readonly HashSet<PoolPrefabItem> _cancelled = new();

        static bool _running;

        [RuntimeInitializeOnLoadMethod]
        static void Boot() => StartLoopIfNeeded();

        static void StartLoopIfNeeded()
        {
            if (_running) return;
            Loop().Forget();
        }

        public static int PendingCount => _queue.Count;

        public static void Enqueue(PoolPrefabItem item)
        {
            if (item == null || item.Config == null || PoolPrefabGlobal.IsReleased(item) || !_inQueue.Add(item)) return;
            
            _queue.Enqueue(item);
            StartLoopIfNeeded();
        }

        public static bool Cancel(PoolPrefabItem item)
        {
            if (!item) return false;
            if (!_inQueue.Contains(item)) return false;
            _cancelled.Add(item);
            return true;
        }

        public static void Clear()
        {
            _queue.Clear();
            _inQueue.Clear();
            _cancelled.Clear();
        }

        public static bool IsEnqueued(PoolPrefabItem item) => item && _inQueue.Contains(item);

        public static async UniTask FlushAsync(int maxPerFrame = 128)
        {
            int processedThisFrame = 0;

            while (_queue.Count > 0)
            {
                var item = _queue.Dequeue();

                if (!item)
                {
                    _inQueue.Remove(item); 
                    continue;
                }

                if (_cancelled.Remove(item))
                {
                    _inQueue.Remove(item); 
                    continue;
                }

                await TaskRelease(item);

                if (!item)
                {
                    _inQueue.Remove(item);
                    continue;
                }

                if (PoolPrefabGlobal.IsReleased(item))
                {
                    _inQueue.Remove(item);
                    continue;
                }

                // ✅ cleanup xong, mark thật sự released
                PoolPrefabGlobal.MarkAsReleased(item);

                _inQueue.Remove(item);

                processedThisFrame++;
                if (processedThisFrame >= maxPerFrame)
                {
                    processedThisFrame = 0;
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
            }
        }

        static async UniTaskVoid Loop()
        {
            _running = true;
            try
            {
                while (Application.isPlaying)
                {
                    if (_queue.Count == 0)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update);
                        continue;
                    }

                    int budget = ItemsPerFrame;
                    while (budget-- > 0 && _queue.Count > 0)
                    {
                        var item = _queue.Dequeue();

                        if (!item) { _inQueue.Remove(item); continue; }
                        if (_cancelled.Remove(item)) { _inQueue.Remove(item); continue; }

                        await TaskRelease(item);

                        if (!item) { _inQueue.Remove(item); continue; }
                        if (PoolPrefabGlobal.IsReleased(item)) { _inQueue.Remove(item); continue; }

                        PoolPrefabGlobal.MarkAsReleased(item);

                        _inQueue.Remove(item);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }
            finally
            {
                _running = false;
            }
        }

        static async UniTask TaskRelease(PoolPrefabItem item)
        {
            if (!item) return;

            var taskRelease = item.GetCachedInterfaces<IPoolRelease>();
            var taskReleaseAsync = item.GetCachedInterfaces<IPoolReleaseAync>();

            int taskCount = 0;
            for (int i = 0; i < taskRelease.Count; i++)
            {
                taskRelease[i].TaskBeforeRelease();

                if (taskCount++ % TaskPerFrame == 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                    taskCount = 0;
                }
            }
            
            for (int i = 0; i < taskReleaseAsync.Count; i++)
            {
                var task = taskReleaseAsync[i];

                await task.TaskBeforeRelease();

                if (taskCount++ % TaskPerFrame == 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                    taskCount = 0;
                }
            }

            await UniTask.Yield();
        }
    }
}
