using UnityEngine;

namespace SketchEngine.Optimize
{
    // Cùng hành vi với OptCollisionSensor2D, khác base class MonoCachedFast.
    public abstract class OptCollisionSensorFast2D : SketchEngine.MonoCachedFast
    {
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!OptCollisionLookup2D.TryGetSlot(other, out OptSlot slot)) return;
            OptCollisionLookup2D.BeginDispatch();
            try
            {
                int n = slot.Count;
                for (int i = 0; i < n; i++)
                {
                    MonoBehaviour owner = slot.Items[i];
                    if (owner != null) OnTriggerEnterOwner(owner);
                }
            }
            finally { OptCollisionLookup2D.EndDispatch(); }
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!OptCollisionLookup2D.TryGetSlot(other, out OptSlot slot)) return;
            OptCollisionLookup2D.BeginDispatch();
            try
            {
                int n = slot.Count;
                for (int i = 0; i < n; i++)
                {
                    MonoBehaviour owner = slot.Items[i];
                    if (owner != null) OnTriggerExitOwner(owner);
                }
            }
            finally { OptCollisionLookup2D.EndDispatch(); }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (!OptCollisionLookup2D.TryGetSlot(collision.collider, out OptSlot slot)) return;
            OptCollisionLookup2D.BeginDispatch();
            try
            {
                int n = slot.Count;
                for (int i = 0; i < n; i++)
                {
                    MonoBehaviour owner = slot.Items[i];
                    if (owner != null) OnCollisionEnterOwner(owner);
                }
            }
            finally { OptCollisionLookup2D.EndDispatch(); }
        }

        protected virtual void OnCollisionExit2D(Collision2D collision)
        {
            if (!OptCollisionLookup2D.TryGetSlot(collision.collider, out OptSlot slot)) return;
            OptCollisionLookup2D.BeginDispatch();
            try
            {
                int n = slot.Count;
                for (int i = 0; i < n; i++)
                {
                    MonoBehaviour owner = slot.Items[i];
                    if (owner != null) OnCollisionExitOwner(owner);
                }
            }
            finally { OptCollisionLookup2D.EndDispatch(); }
        }

        protected virtual void OnTriggerEnterOwner(MonoBehaviour owner) { }
        protected virtual void OnTriggerExitOwner(MonoBehaviour owner) { }
        protected virtual void OnCollisionEnterOwner(MonoBehaviour owner) { }
        protected virtual void OnCollisionExitOwner(MonoBehaviour owner) { }
    }

    public abstract class OptCollisionSensorFast2D<T> : OptCollisionSensorFast2D where T : class
    {
        protected sealed override void OnTriggerEnterOwner(MonoBehaviour owner)
        {
            if (owner is T target) OnTriggerEnterFunc(target);
        }

        protected sealed override void OnTriggerExitOwner(MonoBehaviour owner)
        {
            if (owner is T target) OnTriggerExitFunc(target);
        }

        protected sealed override void OnCollisionEnterOwner(MonoBehaviour owner)
        {
            if (owner is T target) OnCollisionEnterFunc(target);
        }

        protected sealed override void OnCollisionExitOwner(MonoBehaviour owner)
        {
            if (owner is T target) OnCollisionExitFunc(target);
        }

        protected virtual void OnTriggerEnterFunc(T target) { }
        protected virtual void OnTriggerExitFunc(T target) { }
        protected virtual void OnCollisionEnterFunc(T target) { }
        protected virtual void OnCollisionExitFunc(T target) { }
    }
}
