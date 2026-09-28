using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Optimize
{
    /// <summary>
    /// Register một component như ba gameplay type T1, T2, T3 dùng cùng collider set.
    /// </summary>
    public abstract class OptCollisionRegister<T1, T2, T3> : MonoCached
        where T1 : class
        where T2 : class
        where T3 : class
    {
        [SerializeField] protected Collider[] _colliders;
        [SerializeField] protected bool _manuallyAssignColliders = false;

        protected virtual void Awake()
        {
            if (_colliders == null || _colliders.Length == 0)
                _colliders = GetComponentsInChildren<Collider>(true);
        }

        protected virtual void OnEnable()
        {
            if (_colliders == null || _colliders.Length == 0) return;
            OptCollisionLookup.Register<T1>((T1)(object)this, _colliders);
            OptCollisionLookup.Register<T2>((T2)(object)this, _colliders);
            OptCollisionLookup.Register<T3>((T3)(object)this, _colliders);
        }

        protected virtual void OnDisable()
        {
            if (_colliders == null || _colliders.Length == 0) return;
            OptCollisionLookup.Unregister<T1>((T1)(object)this, _colliders);
            OptCollisionLookup.Unregister<T2>((T2)(object)this, _colliders);
            OptCollisionLookup.Unregister<T3>((T3)(object)this, _colliders);
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
