using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SketchEngine.Optimize
{
    public static class OptScriptLookup
    {
        static readonly Dictionary<Collider, List<MonoBehaviour>> _map = new Dictionary<Collider, List<MonoBehaviour>>(2048);
        static readonly Stack<List<MonoBehaviour>> _listPool = new Stack<List<MonoBehaviour>>(64);
        const int MaxPooledLists = 512;
        static readonly object _sync = new object();

        // Initialize runtime subscriptions in a Unity-safe timing window.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void InitOnRuntimeLoad()
        {
            // Subscribe to sceneUnloaded so we clear registrations for the old scene
            // before the new scene's GameObjects run their OnEnable.
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        static void OnSceneUnloaded(Scene scene)
        {
            Clear();
        }

        static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            Clear();
        }

        public static void Register<T>(T owner, Collider collider) where T : MonoBehaviour
        {
            if (owner == null || collider == null)
                return;

            lock (_sync)
            {
                if (!_map.TryGetValue(collider, out var list))
                {
                    list = GetPooledList();
                    _map[collider] = list;
                }

                // avoid duplicate reference
                for (int i = 0; i < list.Count; i++)
                {
                    if (ReferenceEquals(list[i], owner))
                        return;
                }

                list.Add(owner);
            }
        }

        // Convenience params overload so callers can pass multiple colliders inline
        public static void Register<T>(T owner, params Collider[] colliders) where T : MonoBehaviour
        {
            if (owner == null || colliders == null)
                return;

            for (int i = 0; i < colliders.Length; i++)
                Register(owner, colliders[i]);
        }

        public static void Unregister<T>(T owner, Collider collider) where T : MonoBehaviour
        {
            if (owner == null || collider == null)
                return;

            lock (_sync)
            {
                if (_map.TryGetValue(collider, out var list))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (ReferenceEquals(list[i], owner))
                        {
                            list.RemoveAt(i);
                            if (list.Count == 0)
                            {
                                _map.Remove(collider);
                                ReleasePooledList(list);
                            }
                            return;
                        }
                    }
                }
            }
        }

        // Convenience params overload so callers can pass multiple colliders inline
        public static void Unregister<T>(T owner, params Collider[] colliders) where T : MonoBehaviour
        {
            if (owner == null || colliders == null)
                return;

            for (int i = 0; i < colliders.Length; i++)
                Unregister(owner, colliders[i]);
        }

        // Fast no-alloc single result lookup
        public static bool TryFindFirst<T>(Collider collider, out T owner) where T : MonoBehaviour
        {
            owner = null;
            if (collider == null)
                return false;

            lock (_sync)
            {
                if (_map.TryGetValue(collider, out var list) && list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] is T t)
                        {
                            owner = t;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // Fill provided list with matching scripts (caller-provided to avoid allocations)
        public static bool TryGetAll<T>(Collider collider, List<T> results) where T : MonoBehaviour
        {
            if (results == null)
                return false;

            results.Clear();
            if (collider == null)
                return false;

            lock (_sync)
            {
                if (_map.TryGetValue(collider, out var list) && list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] is T t)
                            results.Add(t);
                    }
                }
            }

            return results.Count > 0;
        }

        // Direct access to raw registered MonoBehaviours for this collider (no allocation)
        public static bool TryGetRaw(Collider collider, out IReadOnlyList<MonoBehaviour> owners)
        {
            owners = null;
            if (collider == null)
                return false;

            lock (_sync)
            {
                if (_map.TryGetValue(collider, out var list) && list != null && list.Count > 0)
                {
                    owners = list;
                    return true;
                }
            }

            return false;
        }

        public static void Clear()
        {
            lock (_sync)
            {
                // release all lists to the pool to avoid GC churn
                foreach (var kv in _map)
                {
                    ReleasePooledList(kv.Value);
                }

                _map.Clear();
            }
        }

        static List<MonoBehaviour> GetPooledList()
        {
            lock (_sync)
            {
                if (_listPool.Count > 0)
                    return _listPool.Pop();
            }

            return new List<MonoBehaviour>(1);
        }

        static void ReleasePooledList(List<MonoBehaviour> list)
        {
            list.Clear();
            lock (_sync)
            {
                if (_listPool.Count < MaxPooledLists)
                    _listPool.Push(list);
            }
        }
    }
}
