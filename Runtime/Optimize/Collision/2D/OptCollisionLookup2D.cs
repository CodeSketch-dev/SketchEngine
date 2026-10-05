using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SketchEngine.Optimize
{
    public static class OptCollisionLookup2D
    {
        // Trả về mảng ID đã đăng ký (tái sử dụng cachedIds nếu đúng kích thước).
        public static int[] Register(MonoBehaviour owner, Collider2D[] colliders, int[] cachedIds)
        {
            if (owner == null || colliders == null) return cachedIds;

            int count = colliders.Length;
            if (cachedIds == null || cachedIds.Length != count)
                cachedIds = new int[count];

            for (int i = 0; i < count; i++)
            {
                Collider2D col = colliders[i];
                if (col == null)
                {
                    cachedIds[i] = 0;
                    continue;
                }

                int id = col.GetInstanceID();
                cachedIds[i] = id;
                OptCollisionMap.Add(id, owner);
            }

            return cachedIds;
        }

        // ID 0 = không có collider.
        public static void Unregister(MonoBehaviour owner, int[] ids)
        {
            if (owner == null || ids == null) return;

            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] != 0)
                    OptCollisionMap.Remove(ids[i], owner);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryGetSlot(Collider2D collider, out OptSlot slot)
        {
            slot = null;
            return collider != null && OptCollisionMap.TryGet(collider.GetInstanceID(), out slot);
        }

        // Dùng cho Physics2D.OverlapXxx / Raycast: lấy script kiểu T gắn trên collider mà không GetComponent.
        public static bool TryFindFirst<T>(Collider2D collider, out T result) where T : class
        {
            result = null;
            if (!TryGetSlot(collider, out OptSlot slot)) return false;

            int n = slot.Count;
            for (int i = 0; i < n; i++)
            {
                if (slot.Items[i] is T match)
                {
                    result = match;
                    return true;
                }
            }

            return false;
        }

        // Ghi mọi script kiểu T của collider vào list do caller cấp (không tạo list mới).
        public static bool TryGetAll<T>(Collider2D collider, List<T> results) where T : class
        {
            if (results == null) return false;
            results.Clear();

            if (!TryGetSlot(collider, out OptSlot slot)) return false;

            int n = slot.Count;
            for (int i = 0; i < n; i++)
            {
                if (slot.Items[i] is T match)
                    results.Add(match);
            }

            return results.Count > 0;
        }

        internal static void BeginDispatch() => OptCollisionMap.BeginDispatch();
        internal static void EndDispatch() => OptCollisionMap.EndDispatch();

        public static void Clear()
        {
            OptCollisionMap.Clear();
        }
    }
}
