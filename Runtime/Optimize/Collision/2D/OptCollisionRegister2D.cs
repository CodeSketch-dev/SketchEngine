using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Optimize
{
    /// <summary>
    /// Kế thừa class này rồi implement bất kỳ số interface nào. Không generic, không giới hạn số type.
    /// </summary>
    public abstract class OptCollisionRegister2D : MonoCached
    {
        [SerializeField] protected Collider2D[] _colliders;
        [SerializeField] protected bool _manuallyAssignColliders;

        int[] _colliderIds;

        protected virtual void Awake()
        {
            if (_colliders == null || _colliders.Length == 0)
                _colliders = GetComponentsInChildren<Collider2D>(true);
        }

        protected virtual void OnEnable()
        {
            if (_colliders == null || _colliders.Length == 0) return;
            _colliderIds = OptCollisionLookup2D.Register(this, _colliders, _colliderIds);
        }

        protected virtual void OnDisable()
        {
            OptCollisionLookup2D.Unregister(this, _colliderIds);
        }

        protected virtual void OnValidate()
        {
            if (!Application.isPlaying && !_manuallyAssignColliders)
                _colliders = GetComponentsInChildren<Collider2D>(true);
        }
    }
}
