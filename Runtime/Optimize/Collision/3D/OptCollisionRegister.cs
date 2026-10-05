using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Optimize
{
    /// <summary>
    /// Kế thừa class này rồi implement bất kỳ số interface nào (ICharacterCollidable, IMonsterCollidable, ...).
    /// Object tự đăng ký collider vào bảng chung lúc bật, và tự gỡ lúc tắt. Không generic, không giới hạn số type.
    /// </summary>
    public abstract class OptCollisionRegister : MonoCached
    {
        [SerializeField] protected Collider[] _colliders;
        [SerializeField] protected bool _manuallyAssignColliders = false;

        int[] _colliderIds;

        protected virtual void Awake()
        {
            if (_colliders == null || _colliders.Length == 0)
                _colliders = GetComponentsInChildren<Collider>(true);
        }

        protected virtual void OnEnable()
        {
            if (_colliders == null || _colliders.Length == 0) return;
            _colliderIds = OptCollisionLookup.Register(this, _colliders, _colliderIds);
        }

        protected virtual void OnDisable()
        {
            OptCollisionLookup.Unregister(this, _colliderIds);
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
