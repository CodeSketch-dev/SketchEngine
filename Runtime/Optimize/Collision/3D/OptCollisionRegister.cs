using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Optimize
{
    /// <summary>
    /// Base class:
    /// - Auto register colliders to OptCollisionLookup
    /// - T = interface / base gameplay type that the concrete subclass implements
    /// - Zero GetComponent in runtime
    /// </summary>
    public abstract class OptCollisionRegister<T> : MonoCached where T : class
    {
        [SerializeField] protected Collider[] _colliders;
        [SerializeField] protected bool _manuallyAssignColliders = false;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        protected virtual void Awake()
        {
            if (_colliders == null || _colliders.Length == 0)
                _colliders = GetComponentsInChildren<Collider>(true);
        }

        protected virtual void OnEnable()
        {
            if (_colliders == null || _colliders.Length == 0) return;
            OptCollisionLookup.Register<T>((T)(object)this, _colliders);
        }

        protected virtual void OnDisable()
        {
            if (_colliders == null || _colliders.Length == 0) return;
            OptCollisionLookup.Unregister<T>((T)(object)this, _colliders);
        }

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && !_manuallyAssignColliders)
                _colliders = GetComponentsInChildren<Collider>(true);
#endif
        }
    }
}
