using System;
using UnityEngine;
using SketchEngine.Mono;

namespace SketchEngine.Optimize
{
    /// <summary>
    /// Base class xử lý va chạm với các object đã register vào OptCollisionLookup theo type T.
    /// - Zero GetComponent, zero GC runtime
    /// - Delegates lazy-cached sau lần collision đầu tiên — không allocate mỗi event
    /// - Không cần gọi base.Awake()
    /// </summary>
    public abstract class OptCollisionSensor<T> : MonoCached
        where T : class
    {
        // Lazy-cached on first collision: allocation happens once, never again.
        // Safe even if subclass does not call base.Awake().
        Action<T> _cachedCollisionEnter;
        Action<T> _cachedCollisionExit;
        Action<T> _cachedTriggerEnter;
        Action<T> _cachedTriggerExit;

        // ==================== COLLISION ====================

        protected virtual void OnCollisionEnter(Collision collision)
        {
            var col = collision.collider;
            if (col == null) return;
            _cachedCollisionEnter ??= OnCollisionEnterFunc;
            OptCollisionLookup.ForEach<T>(col, _cachedCollisionEnter);
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            var col = collision.collider;
            if (col == null) return;
            _cachedCollisionExit ??= OnCollisionExitFunc;
            OptCollisionLookup.ForEach<T>(col, _cachedCollisionExit);
        }

        // ==================== TRIGGER ======================

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            _cachedTriggerEnter ??= OnTriggerEnterFunc;
            OptCollisionLookup.ForEach<T>(other, _cachedTriggerEnter);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other == null) return;
            _cachedTriggerExit ??= OnTriggerExitFunc;
            OptCollisionLookup.ForEach<T>(other, _cachedTriggerExit);
        }

        // ==================== GAMEPLAY CALLBACKS ============

        protected virtual void OnCollisionEnterFunc(T target) { }
        protected virtual void OnCollisionExitFunc(T target) { }
        protected virtual void OnTriggerEnterFunc(T target) { }
        protected virtual void OnTriggerExitFunc(T target) { }
    }
}
