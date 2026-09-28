using System;
using System.Collections.Generic;
using UnityEngine;

using SketchEngine.Mono;

namespace SketchEngine.Core
{
    /// <summary>
    /// MonoBehaviour base class with strict one-time component & interface resolution.
    ///
    /// Features:
    /// - Resolve each type exactly once
    /// - Support single and multiple results
    /// - Support Component and Interface
    /// - No repeated GetComponent / GetComponents
    ///
    /// Guarantees:
    /// - No exceptions
    /// - No invalid casts
    /// - No null caching
    /// - Clear, debuggable state
    /// </summary>
    public class MonoBaseOpt : MonoBase
    {
        Dictionary<Type, Component[]> _cache;
        HashSet<Type> _resolved;

        void EnsureInit()
        {
            _cache    ??= new Dictionary<Type, Component[]>(4);
            _resolved ??= new HashSet<Type>();
        }

        // =====================================================
        // Component (single)
        // =====================================================

        public T GetCached<T>() where T : Component
        {
            EnsureInit();
            var type = typeof(T);

            if (_resolved.Contains(type))
            {
                return _cache.TryGetValue(type, out var arr) && arr.Length > 0
                    ? arr[0] as T
                    : null;
            }

            var comp = GetComponent<T>();
            if (comp != null)
                _cache[type] = new Component[] { comp };

            _resolved.Add(type);
            return comp;
        }

        // =====================================================
        // Interface (single – first match)
        // =====================================================

        public T GetCachedInterface<T>() where T : class
        {
            EnsureInit();
            var type = typeof(T);

            if (_resolved.Contains(type))
            {
                return _cache.TryGetValue(type, out var arr) && arr.Length > 0
                    ? arr[0] as T
                    : null;
            }

            var components = GetComponents<Component>();
            Component found = null;

            foreach (var c in components)
            {
                if (c is T)
                {
                    found = c;
                    break;
                }
            }

            if (found != null)
                _cache[type] = new Component[] { found };

            _resolved.Add(type);
            return found as T;
        }

        // =====================================================
        // Interface (multiple)
        // =====================================================

        public IReadOnlyList<T> GetCachedInterfaces<T>() where T : class
        {
            EnsureInit();
            var type = typeof(T);

            if (_resolved.Contains(type))
            {
                if (_cache.TryGetValue(type, out var arr) && arr.Length > 0)
                {
                    var result = new List<T>(arr.Length);
                    foreach (var c in arr)
                        if (c is T iface)
                            result.Add(iface);

                    return result;
                }

                return Array.Empty<T>();
            }

            var components = GetComponents<Component>();
            List<Component> matches = null;
            List<T> interfaces = null;

            foreach (var c in components)
            {
                if (c is T iface)
                {
                    matches ??= new List<Component>(2);
                    interfaces ??= new List<T>(2);

                    matches.Add(c);
                    interfaces.Add(iface);
                }
            }

            if (matches != null)
                _cache[type] = matches.ToArray();

            _resolved.Add(type);
            return interfaces ?? (IReadOnlyList<T>)Array.Empty<T>();
        }

        // =====================================================
        // Cache control
        // =====================================================

        protected void ClearComponentCache()
        {
            _cache?.Clear();
            _resolved?.Clear();
        }
    }
}
