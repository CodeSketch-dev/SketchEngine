using System;
using UnityEngine;

namespace SketchEngine.Optimize
{
    /// <summary>
    /// OptCollisionSensor kế thừa MonoCachedFast — có thêm GetCached&lt;T&gt;() component caching.
    /// Dùng khi sensor cần GetComponent không-GC trên cùng GameObject.
    /// </summary>
    public abstract class OptCollisionSensorFast<T> : SketchEngine.MonoCachedFast
        where T : class
    {
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
