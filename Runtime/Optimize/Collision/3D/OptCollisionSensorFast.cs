using UnityEngine;

namespace SketchEngine.Optimize
{
    // Cùng hành vi với OptCollisionSensor, khác base class MonoCachedFast (cache component nhanh hơn).
    public abstract class OptCollisionSensorFast : SketchEngine.MonoCachedFast
    {
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!OptCollisionLookup.TryGetSlot(collision.collider, out OptSlot slot)) return;
            OptCollisionLookup.BeginDispatch();
            try
            {
                int n = slot.Count;
                for (int i = 0; i < n; i++)
                {
                    MonoBehaviour owner = slot.Items[i];
                    if (owner != null) OnCollisionEnterOwner(owner);
                }
            }
            finally { OptCollisionLookup.EndDispatch(); }
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (!OptCollisionLookup.TryGetSlot(collision.collider, out OptSlot slot)) return;
            OptCollisionLookup.BeginDispatch();
            try
            {
                int n = slot.Count;
                for (int i = 0; i < n; i++)
                {
                    MonoBehaviour owner = slot.Items[i];
                    if (owner != null) OnCollisionExitOwner(owner);
                }
            }
            finally { OptCollisionLookup.EndDispatch(); }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!OptCollisionLookup.TryGetSlot(other, out OptSlot slot)) return;
            OptCollisionLookup.BeginDispatch();
            try
            {
                int n = slot.Count;
                for (int i = 0; i < n; i++)
                {
                    MonoBehaviour owner = slot.Items[i];
                    if (owner != null) OnTriggerEnterOwner(owner);
                }
            }
            finally { OptCollisionLookup.EndDispatch(); }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (!OptCollisionLookup.TryGetSlot(other, out OptSlot slot)) return;
            OptCollisionLookup.BeginDispatch();
            try
            {
                int n = slot.Count;
                for (int i = 0; i < n; i++)
                {
                    MonoBehaviour owner = slot.Items[i];
                    if (owner != null) OnTriggerExitOwner(owner);
                }
            }
            finally { OptCollisionLookup.EndDispatch(); }
        }

        protected virtual void OnCollisionEnterOwner(MonoBehaviour owner) { }
        protected virtual void OnCollisionExitOwner(MonoBehaviour owner) { }
        protected virtual void OnTriggerEnterOwner(MonoBehaviour owner) { }
        protected virtual void OnTriggerExitOwner(MonoBehaviour owner) { }
    }

    public abstract class OptCollisionSensorFast<T> : OptCollisionSensorFast where T : class
    {
        protected sealed override void OnCollisionEnterOwner(MonoBehaviour owner)
        {
            if (owner is T target) OnCollisionEnterFunc(target);
        }

        protected sealed override void OnCollisionExitOwner(MonoBehaviour owner)
        {
            if (owner is T target) OnCollisionExitFunc(target);
        }

        protected sealed override void OnTriggerEnterOwner(MonoBehaviour owner)
        {
            if (owner is T target) OnTriggerEnterFunc(target);
        }

        protected sealed override void OnTriggerExitOwner(MonoBehaviour owner)
        {
            if (owner is T target) OnTriggerExitFunc(target);
        }

        protected virtual void OnCollisionEnterFunc(T target) { }
        protected virtual void OnCollisionExitFunc(T target) { }
        protected virtual void OnTriggerEnterFunc(T target) { }
        protected virtual void OnTriggerExitFunc(T target) { }
    }
}
