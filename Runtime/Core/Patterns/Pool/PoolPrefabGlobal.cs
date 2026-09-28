using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

using SketchEngine.Mono;

namespace SketchEngine.Patterns.Pool
{
    public static class PoolPrefabGlobal
    {
        static readonly Dictionary<PoolPrefabConfig, PoolPrefab> _poolLookup = new();

        static readonly HashSet<GameObject> s_Released = new();
        static readonly HashSet<GameObject> s_PendingInstances = new();
        static readonly HashSet<GameObject> s_Actives = new();

        static readonly Dictionary<GameObject, PoolPrefabItem> _cache = new();

        public static int ActiveCount => s_Actives.Count;

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            MonoCallback.SafeInstance.EventActiveSceneChanged += MonoCallback_EventActiveSceneChanged;
        }

        static void MonoCallback_EventActiveSceneChanged(Scene cur, Scene next)
        {
            Clean();
        }

        public static void Clean()
        {
            PoolReleaseScheduler.Clear();

            s_Released.Clear();
            s_Actives.Clear();
            s_PendingInstances.Clear();

            foreach (var pool in _poolLookup.Values)
            {
                if (pool.Config != null && pool.Config.PersistAcrossScenes) continue;

                pool.Clear();
                Pooler.Release(pool.Config);
            }
        }

        public static void Init(params PoolPrefabConfig[] configs)
        {
            if (configs == null || configs.Length == 0)
                return;

            for (int i = 0; i < configs.Length; i++)
            {
                var config = configs[i];
                if (config == null || !config.Prefab) continue;

                if (!_poolLookup.ContainsKey(config))
                {
                    try
                    {
                        _poolLookup.Add(config, new PoolPrefab(config));
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"PoolPrefabGlobal: Init failed for config {config.name}. {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }

        // =========================================================
        public static PoolPrefabItem Get(PoolPrefabConfig config)
        {
            if (config == null) return null;

            var item = GetPool(config).Get();
            if (item == null) return null;

            var go = item.GameObjectCached;
            _cache[go] = item;

            s_Released.Remove(go);
            s_PendingInstances.Remove(go);
            s_Actives.Add(go);
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(PoolPrefabConfig config) where T : Component
        {
            var item = Get(config);
            return item.GetCached<T>();
        }

        // =========================================================
        public static void Release(PoolPrefabConfig config, GameObject instance)
        {
            if (config == null || !instance) return;
            if (s_Released.Contains(instance) || s_PendingInstances.Contains(instance)) return;

            if (config.DeactiveOnRelease)
                instance.SetActive(false);

            var item = _cache.GetValueOrDefault(instance);
            if (item != null)
            {
                var taskRelease = item.GetCachedInterfaces<IPoolRelease>();
                var taskReleaseAsync = item.GetCachedInterfaces<IPoolReleaseAync>();

                // If any of the interfaces implement IPoolRelease or IPoolReleaseAync, we need to schedule the release.
                // Otherwise, we can release immediately.
                // This case for smooth fps
                if ((taskRelease != null && taskRelease.Count > 0) || (taskReleaseAsync != null && taskReleaseAsync.Count > 0))
                {
                    s_PendingInstances.Add(instance);
                    s_Actives.Remove(instance);
                    PoolReleaseScheduler.Enqueue(item);
                }
                else
                {
                    s_Released.Add(instance);
                    s_Actives.Remove(instance);
                    GetPool(config).Release(item);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release<T>(PoolPrefabConfig cfg, T inst) where T : Component
        {
            if (!inst) return;
            Release(cfg, inst.gameObject);
        }

        public static PoolPrefab GetPool(PoolPrefabConfig config)
        {
            if (!_poolLookup.TryGetValue(config, out var pool))
            {
                pool = new PoolPrefab(config);
                _poolLookup.Add(config, pool);
            }
            return pool;
        }

        // =========================================================
        public static bool IsReleased(PoolPrefabItem ins)
        {
            return ins && s_Released.Contains(ins.GameObjectCached);
        }

        public static bool IsPending(PoolPrefabItem ins)
        {
            return ins && s_PendingInstances.Contains(ins.GameObjectCached);
        }

        internal static void NotifyDestroyed(PoolPrefabItem ins)
        {
            if (!ins) return;

            var instance = ins.GameObjectCached;
            if (!instance) return;

            PoolReleaseScheduler.Cancel(ins);

            s_PendingInstances.Remove(instance);
            s_Released.Remove(instance);
            s_Actives.Remove(instance);
            _cache.Remove(instance);
        }

        internal static void MarkAsReleased(PoolPrefabItem ins)
        {
            if (!ins) return;

            s_PendingInstances.Remove(ins.GameObjectCached);
            s_Released.Add(ins.GameObjectCached);
            _cache.Remove(ins.GameObjectCached);

            GetPool(ins.Config).Release(ins);
        }

        // =========================================================
        public static void DestroyPools(IList<PoolPrefabConfig> configs)
        {
            if (configs == null || configs.Count == 0) return;

            foreach (var config in configs)
            {
                if (config == null) continue;
                if (_poolLookup.TryGetValue(config, out var pool))
                {
                    pool.DestroyAll();
                    _poolLookup.Remove(config);
                }
            }

            // 🧹 Clean orphan references
            s_Actives.RemoveWhere(x => x == null);
            s_Released.RemoveWhere(x => x == null);
            s_PendingInstances.RemoveWhere(x => x == null);
        }
    }
}
