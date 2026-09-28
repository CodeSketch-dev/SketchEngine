using System;
using System.Collections.Generic;
using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine
{
    public abstract class MonoCachedFast : MonoCached
    {
        readonly Dictionary<Type, Component> _componentCache = new();
        readonly HashSet<Type> _missingCache = new();

        public T GetCached<T>(bool includeInactive = true, bool retryIfMissing = true)
        where T : Component
        {
            Type type = typeof(T);

            if (_componentCache.TryGetValue(type, out Component cached))
            {
                if (cached != null)
                    return (T)cached;

                _componentCache.Remove(type);
            }

            if (!retryIfMissing && _missingCache.Contains(type))
                return null;

            T found = GetComponent<T>();

            if (found == null)
                found = GetComponentInChildren<T>(includeInactive);

            if (found != null)
            {
                _componentCache[type] = found;
                _missingCache.Remove(type);
            }
            else
            {
                _missingCache.Add(type);
            }

            return found;
        }

        public void ClearCached<T>() where T : Component
        {
            Type type = typeof(T);
            _componentCache.Remove(type);
            _missingCache.Remove(type);
        }

        public void ClearAllCached()
        {
            _componentCache.Clear();
            _missingCache.Clear();
        }
    }
}
