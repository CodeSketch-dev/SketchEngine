using System;
using UnityEngine;

namespace SketchEngine.Mono
{
    public class MonoBase : MonoBehaviour
    {
        GameObject _gameObject;
        Transform _transform;
        RectTransform _rectTransform;

        Action _cachedTick;
        Action _cachedLateTick;
        Action _cachedFixedTick;
        bool _tickRegistered;

        bool _isStarted = false;

        public Transform TransformCached
        {
            get
            {
                if (!_transform)
                    _transform = transform;

                return _transform;
            }
        }

        public RectTransform RectTransformCached
        {
            get
            {
                if (!_rectTransform)
                    _rectTransform = TransformCached as RectTransform;
                return _rectTransform;
            }
        }

        public GameObject GameObjectCached
        {
            get
            {
                if (!_gameObject)
                    _gameObject = gameObject;

                return _gameObject;
            }
        }

        protected virtual void OnEnable()
        {
            // Prevents tick functions called before "Start"
            if (!_isStarted)
                return;

            RegisterTick();
        }

        protected virtual void OnDisable()
        {
            if (!_isStarted)
                return;

            UnregisterTick();
        }

        protected virtual void Start()
        {
            _isStarted = true;

            RegisterTick();
        }

        void RegisterTick()
        {
            if (_tickRegistered) return;

            if (_cachedTick == null) _cachedTick = Tick;
            if (_cachedLateTick == null) _cachedLateTick = LateTick;
            if (_cachedFixedTick == null) _cachedFixedTick = FixedTick;

            MonoCallback.SafeInstance.EventUpdate += _cachedTick;
            MonoCallback.SafeInstance.EventLateUpdate += _cachedLateTick;
            MonoCallback.SafeInstance.EventFixedUpdate += _cachedFixedTick;

            _tickRegistered = true;
        }

        void UnregisterTick()
        {
            if (!_tickRegistered) return;

            if (MonoCallback.IsDestroyed)
                return;

            MonoCallback.Instance.EventUpdate -= _cachedTick;
            MonoCallback.Instance.EventLateUpdate -= _cachedLateTick;
            MonoCallback.Instance.EventFixedUpdate -= _cachedFixedTick;

            _tickRegistered = false;
        }

        protected virtual void Awake()
        {
            // Cache delegates to avoid per-enable allocations
            _cachedTick = Tick;
            _cachedLateTick = LateTick;
            _cachedFixedTick = FixedTick;
        }

        protected virtual void Tick()
        {
        }

        protected virtual void LateTick()
        {
        }

        protected virtual void FixedTick()
        {
        }
    }
}