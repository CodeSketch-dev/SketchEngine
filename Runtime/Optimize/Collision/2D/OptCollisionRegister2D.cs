using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Optimize
{
    public abstract class OptCollisionRegister2D<T> : MonoCached where T : class
    {
        [SerializeField] protected Collider2D[] _colliders;
        [SerializeField] protected bool _manuallyAssignColliders;

        protected virtual void Awake()
        {
            if (_colliders == null || _colliders.Length == 0)
                _colliders = GetComponentsInChildren<Collider2D>(true);
        }

        protected virtual void OnEnable() => OptCollisionLookup2D.Register<T>((T)(object)this, _colliders);
        protected virtual void OnDisable() => OptCollisionLookup2D.Unregister<T>((T)(object)this, _colliders);

        protected virtual void OnValidate()
        {
            if (!Application.isPlaying && !_manuallyAssignColliders)
                _colliders = GetComponentsInChildren<Collider2D>(true);
        }
    }
}
