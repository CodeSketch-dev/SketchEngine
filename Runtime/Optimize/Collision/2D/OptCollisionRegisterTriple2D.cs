using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Optimize
{
    public abstract class OptCollisionRegisterTriple2D<T1, T2, T3> : MonoCached
        where T1 : class where T2 : class where T3 : class
    {
        [SerializeField] protected Collider2D[] _colliders;
        [SerializeField] protected bool _manuallyAssignColliders;

        protected virtual void Awake()
        {
            if (_colliders == null || _colliders.Length == 0) _colliders = GetComponentsInChildren<Collider2D>(true);
        }

        protected virtual void OnEnable()
        {
            OptCollisionLookup2D.Register<T1>((T1)(object)this, _colliders);
            OptCollisionLookup2D.Register<T2>((T2)(object)this, _colliders);
            OptCollisionLookup2D.Register<T3>((T3)(object)this, _colliders);
        }

        protected virtual void OnDisable()
        {
            OptCollisionLookup2D.Unregister<T1>((T1)(object)this, _colliders);
            OptCollisionLookup2D.Unregister<T2>((T2)(object)this, _colliders);
            OptCollisionLookup2D.Unregister<T3>((T3)(object)this, _colliders);
        }

        protected virtual void OnValidate()
        {
            if (!Application.isPlaying && !_manuallyAssignColliders) _colliders = GetComponentsInChildren<Collider2D>(true);
        }
    }
}
