using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SketchEngine.Optimize
{
    public static class OptCollisionLookup2D
    {
        internal static class TypedMap<T> where T : class
        {
            internal static readonly Dictionary<int, T[]> MAP = new(64);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void Register(int id, T owner)
            {
                if (!MAP.TryGetValue(id, out var owners))
                {
                    MAP[id] = new[] { owner };
                    return;
                }

                for (int i = 0; i < owners.Length; i++)
                    if (ReferenceEquals(owners[i], owner)) return;

                var newOwners = new T[owners.Length + 1];
                Array.Copy(owners, newOwners, owners.Length);
                newOwners[owners.Length] = owner;
                MAP[id] = newOwners;
            }

            internal static void Unregister(int id, T owner)
            {
                if (!MAP.TryGetValue(id, out var owners)) return;

                int index = -1;
                for (int i = 0; i < owners.Length; i++)
                    if (ReferenceEquals(owners[i], owner)) { index = i; break; }
                if (index < 0) return;
                if (owners.Length == 1) { MAP.Remove(id); return; }

                var newOwners = new T[owners.Length - 1];
                if (index > 0) Array.Copy(owners, 0, newOwners, 0, index);
                if (index < owners.Length - 1) Array.Copy(owners, index + 1, newOwners, index, owners.Length - index - 1);
                MAP[id] = newOwners;
            }
        }

        public static void Register<T>(T owner, Collider2D[] colliders) where T : class
        {
            if (owner == null || colliders == null) return;
            for (int i = 0; i < colliders.Length; i++)
                if (colliders[i] != null) TypedMap<T>.Register(colliders[i].GetInstanceID(), owner);
        }

        public static void Unregister<T>(T owner, Collider2D[] colliders) where T : class
        {
            if (owner == null || colliders == null) return;
            for (int i = 0; i < colliders.Length; i++)
                if (colliders[i] != null) TypedMap<T>.Unregister(colliders[i].GetInstanceID(), owner);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(Collider2D collider, Action<T> action) where T : class
        {
            if (collider == null || action == null || !TypedMap<T>.MAP.TryGetValue(collider.GetInstanceID(), out var owners)) return;
            for (int i = 0; i < owners.Length; i++) action(owners[i]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetFirst<T>(Collider2D collider, out T owner) where T : class
        {
            owner = null;
            if (collider == null || !TypedMap<T>.MAP.TryGetValue(collider.GetInstanceID(), out var owners) || owners.Length == 0) return false;
            owner = owners[0];
            return true;
        }
    }
}
