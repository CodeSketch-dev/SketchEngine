using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace SketchEngine.Patterns.Pool
{
    public sealed class PoolPrefab
    {
        readonly GameObject _prefab;
        readonly ObjectPool<PoolPrefabItem> _pool;

        public PoolPrefabConfig Config { get; }


        public PoolPrefab(PoolPrefabConfig config)
        {
            if (config == null || !config.Prefab)
                throw new InvalidOperationException("PoolPrefab: Config hoặc prefab null!");

            Config = config;

            _prefab = config.Prefab;

            _pool = new ObjectPool<PoolPrefabItem>(
                Create, OnGet, OnRelease, OnDestroy,
                false, config.PoolCapacity, config.PoolCapacityMax
            );

            // Prewarm
            var targetPrewarm = Mathf.Min(config.PoolPrewarm, config.PoolCapacityMax);
            if (config.PoolPrewarm > config.PoolCapacityMax)
            {
                Debug.LogWarning($"PoolPrefab: Prewarm ({config.PoolPrewarm}) is greater than max capacity ({config.PoolCapacityMax}) for {config.name}. Clamp to max capacity.");
            }

            int safetyCount = 0;
            int maxSafetyCount = Mathf.Max(8, targetPrewarm * 2 + 8);
            while (_pool.CountInactive < targetPrewarm)
            {
                if (++safetyCount > maxSafetyCount)
                {
                    Debug.LogError($"PoolPrefab: Prewarm safety break for {config.name}. Inactive={_pool.CountInactive}, Target={targetPrewarm}.");
                    break;
                }

                var go = Create();
                if (!go)
                {
                    Debug.LogError($"PoolPrefab: Failed to create pooled item for {config.name}. Ensure prefab has PoolPrefabItem component.");
                    break;
                }

                _pool.Release(go);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        PoolPrefabItem Create()
        {
            PoolPrefabItem item = null;

            if (Application.isPlaying)
            {
                var instance = Object.Instantiate(_prefab);
                item = instance.GetComponent<PoolPrefabItem>();

                if (item == null)
                {
                    Object.Destroy(instance);
                    return null;
                }

                item.GameObjectCached.SetActive(false);
            }
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnGet(PoolPrefabItem item)
        {
            if (!item) return;

            if (Config.PersistAcrossScenes)
            {
                Pooler.Attach(item);
            }
            else
            {
                item.TransformCached.SetParent(null);
            }

            item.GameObjectCached.SetActive(true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnRelease(PoolPrefabItem item)
        {
            if (!item) return;

            // var poolRelease = item.GetCachedInterfaces<IPoolRelease>();
            //
            // foreach (var release in poolRelease)
            // {
            //     release.TaskBeforeRelease();
            // }

            if (!Config.DeactiveOnRelease)
                item.TransformCached.position = new Vector3(99999f, -99999f, 99999f);
            else
                item.GameObjectCached.SetActive(false);

            Pooler.Attach(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void OnDestroy(PoolPrefabItem go)
        {
            if (!go) return;
#if UNITY_EDITOR
            Object.DestroyImmediate(go);
#else
            Object.Destroy(go);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PoolPrefabItem Get()
        {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                var item = _pool.Get();
                if (item)
                    return item;

                var created = Create();
                if (created)
                {
                    _pool.Release(created);
                }
            }

            Debug.LogError($"PoolPrefab: Get failed for {Config.name}. Pool returned null item.");
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(PoolPrefabItem item)
        {
            if (item) _pool.Release(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _pool.Clear();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyAll()
        {
            _pool.Clear();
        }
    }
}
