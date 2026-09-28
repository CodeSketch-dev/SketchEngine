using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SketchEngine.Optimize
{
    public interface IOptCollisionAction<T> where T : class
    {
        void Invoke(T target);
    }

    public static class OptCollisionLookup
    {
        // One Dictionary<int, T[]> per type T (static generic = JIT-specialized, no inner type lookup).
        // Key = Collider.GetInstanceID() — int dict is faster than reference-type key.
        // Value = typed snapshot array — no object cast in hot path.
        internal static class TypedMap<T> where T : class
        {
            internal static readonly Dictionary<int, T[]> Map = new(64);

            static TypedMap() => _clearCallbacks.Add(static () => Map.Clear());

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Register(int id, T owner)
            {
                if (!Map.TryGetValue(id, out var arr))
                {
                    Map[id] = new T[] { owner };
                    return;
                }

                for (int i = 0; i < arr.Length; i++)
                    if (ReferenceEquals(arr[i], owner)) return;

                var newArr = new T[arr.Length + 1];
                Array.Copy(arr, newArr, arr.Length);
                newArr[arr.Length] = owner;
                Map[id] = newArr;
            }

            internal static void Unregister(int id, T owner)
            {
                if (!Map.TryGetValue(id, out var arr)) return;

                int idx = -1;
                for (int i = 0; i < arr.Length; i++)
                    if (ReferenceEquals(arr[i], owner)) { idx = i; break; }

                if (idx < 0) return;

                if (arr.Length == 1) { Map.Remove(id); return; }

                var newArr = new T[arr.Length - 1];
                if (idx > 0) Array.Copy(arr, 0, newArr, 0, idx);
                if (idx < arr.Length - 1) Array.Copy(arr, idx + 1, newArr, idx, arr.Length - idx - 1);
                Map[id] = newArr;
            }
        }

        static readonly List<Action> _clearCallbacks = new(8);

        // =========================================================
        // REGISTER
        // =========================================================

        public static void Register<T>(T owner, Collider[] colliders) where T : class
        {
            if (owner == null || colliders == null) return;
            foreach (var col in colliders)
            {
                if (col == null) continue;
                TypedMap<T>.Register(col.GetInstanceID(), owner);
            }
        }

        public static void Unregister<T>(T owner, Collider[] colliders) where T : class
        {
            if (owner == null || colliders == null) return;
            foreach (var col in colliders)
            {
                if (col == null) continue;
                TypedMap<T>.Unregister(col.GetInstanceID(), owner);
            }
        }

        // =========================================================
        // QUERY – HOT PATH
        // =========================================================

        // Single dict lookup, typed array, no cast.
        // Use collider == null (C# ref check) — Unity guarantees collider alive at collision time.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(Collider collider, Action<T> action) where T : class
        {
            if (collider == null || action == null) return;

            if (!TypedMap<T>.Map.TryGetValue(collider.GetInstanceID(), out var arr)) return;

            for (int i = 0; i < arr.Length; i++)
                action(arr[i]);
        }

        // Zero-alloc variant: pass a struct implementing IOptCollisionAction<T> by ref.
        // Avoids delegate allocation entirely — use when action would capture context.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T, TAction>(Collider collider, ref TAction action)
            where T : class
            where TAction : struct, IOptCollisionAction<T>
        {
            if (collider == null) return;
            if (!TypedMap<T>.Map.TryGetValue(collider.GetInstanceID(), out var arr)) return;

            for (int i = 0; i < arr.Length; i++)
                action.Invoke(arr[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetFirst<T>(Collider collider, out T owner) where T : class
        {
            owner = null;
            if (collider == null) return false;

            if (!TypedMap<T>.Map.TryGetValue(collider.GetInstanceID(), out var arr)) return false;

            if (arr.Length > 0) { owner = arr[0]; return true; }

            return false;
        }

        public static void GetSnapshot<T>(Collider collider, List<T> result) where T : class
        {
            if (result == null) return;
            result.Clear();
            if (collider == null) return;

            if (!TypedMap<T>.Map.TryGetValue(collider.GetInstanceID(), out var arr)) return;

            for (int i = 0; i < arr.Length; i++)
                result.Add(arr[i]);
        }

        // =========================================================

        public static void Clear()
        {
            for (int i = 0; i < _clearCallbacks.Count; i++)
                _clearCallbacks[i].Invoke();
        }
    }
}
