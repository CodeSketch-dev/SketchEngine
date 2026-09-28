using System;
using SketchEngine.Mono;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SketchEngine.Patterns.Pool
{
    public class PoolPrefabItem : MonoCachedOpt
    {
        [Header("Config")]
        [SerializeField] PoolPrefabConfig _config;

        Action<Scene, Scene> _cachedReleaseAction;

        public PoolPrefabConfig Config
        {
            get => _config;
            set => _config = value;
        }

        protected virtual void Awake()
        {
            GetCachedInterfaces<IPoolRelease>();
            GetCachedInterfaces<IPoolReleaseAync>();
        }

        protected virtual void OnEnable()
        {
            TrySubscribe();
        }

        protected virtual void OnDisable()
        {
            TryUnsubscribe();
        }

        protected virtual void OnDestroy()
        {
            TryUnsubscribe();
            PoolPrefabGlobal.NotifyDestroyed(this);
        }

        protected virtual void Start()
        {
            TrySubscribe(); // phòng trường hợp OnEnable chưa kịp
        }

        void TrySubscribe()
        {
            if (_cachedReleaseAction == null) _cachedReleaseAction = MonoCallback_EventActiveSceneChanged;

            if (MonoCallback.HasInstance)
            {
                MonoCallback.Instance.EventActiveSceneChanged += _cachedReleaseAction;
            }
        }

        void TryUnsubscribe()
        {
            if (MonoCallback.HasInstance)
            {
                MonoCallback.Instance.EventActiveSceneChanged -= _cachedReleaseAction;
            }
        }

        void MonoCallback_EventActiveSceneChanged(Scene sceneCurrent, Scene sceneNext)
        {
            // ====== GUARDS RẤT QUAN TRỌNG ======
            if (!this || !gameObject || _config == null || !GameObjectCached) return; // Unity fake-null on destroyed component

            // Nếu đã release vào pool rồi -> bỏ qua
            if (PoolPrefabGlobal.IsReleased(this)) return;

            // Nếu không có mapping prefab/pool -> đừng tự Destroy ở đây,
            // vì scene change sẽ xử lý; tránh double-destroy trong callback.
            if (_config.Prefab == null)
                return;

            // Trả về pool (an toàn idempotent vì Global có _releasedInstances)
            PoolPrefabGlobal.Release(_config, this);
        }
    }
}