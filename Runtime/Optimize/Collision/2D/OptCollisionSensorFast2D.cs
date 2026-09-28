using System;
using UnityEngine;

namespace SketchEngine.Optimize
{
    public abstract class OptCollisionSensorFast2D<T> : SketchEngine.MonoCachedFast where T : class
    {
        Action<T> _triggerEnter;
        Action<T> _triggerExit;
        Action<T> _collisionEnter;
        Action<T> _collisionExit;

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            _triggerEnter ??= OnTriggerEnterFunc;
            OptCollisionLookup2D.ForEach(other, _triggerEnter);
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            _triggerExit ??= OnTriggerExitFunc;
            OptCollisionLookup2D.ForEach(other, _triggerExit);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            _collisionEnter ??= OnCollisionEnterFunc;
            OptCollisionLookup2D.ForEach(collision.collider, _collisionEnter);
        }

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {
            _collisionExit ??= OnCollisionExitFunc;
            OptCollisionLookup2D.ForEach(collision.collider, _collisionExit);
        }

        protected virtual void OnTriggerEnterFunc(T target) { }
        protected virtual void OnTriggerExitFunc(T target) { }
        protected virtual void OnCollisionEnterFunc(T target) { }
        protected virtual void OnCollisionExitFunc(T target) { }
    }
}
