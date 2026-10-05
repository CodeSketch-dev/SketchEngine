using System.Collections.Generic;
using UnityEngine;

namespace SketchEngine.Optimize
{
    // Danh sách owner của một collider. Không copy mảng khi Add/Remove:
    // Remove chỉ đặt null (tombstone), compact khi không đang dispatch.
    internal sealed class OptSlot
    {
        internal MonoBehaviour[] Items = new MonoBehaviour[4];
        internal int Count;
        internal int Live;
    }

    // Bảng chung 2D và 3D: InstanceID của Collider/Collider2D -> OptSlot.
    internal static class OptCollisionMap
    {
        static readonly Dictionary<int, OptSlot> s_map = new Dictionary<int, OptSlot>(64);
        static readonly Stack<OptSlot> s_free = new Stack<OptSlot>(32);
        static int s_dispatchDepth;

        internal static void BeginDispatch()
        {
            s_dispatchDepth++;
        }

        internal static void EndDispatch()
        {
            if (s_dispatchDepth > 0)
                s_dispatchDepth--;
        }

        internal static bool TryGet(int id, out OptSlot slot)
        {
            return s_map.TryGetValue(id, out slot) && slot.Live > 0;
        }

        internal static void Add(int id, MonoBehaviour owner)
        {
            if (!s_map.TryGetValue(id, out OptSlot slot))
            {
                slot = s_free.Count > 0 ? s_free.Pop() : new OptSlot();
                slot.Count = 0;
                slot.Live = 0;
                s_map[id] = slot;
            }

            for (int i = 0; i < slot.Count; i++)
            {
                if (ReferenceEquals(slot.Items[i], owner))
                    return;
            }

            if (slot.Count == slot.Items.Length)
                System.Array.Resize(ref slot.Items, slot.Items.Length * 2);

            slot.Items[slot.Count++] = owner;
            slot.Live++;
        }

        internal static void Remove(int id, MonoBehaviour owner)
        {
            if (!s_map.TryGetValue(id, out OptSlot slot))
                return;

            for (int i = 0; i < slot.Count; i++)
            {
                if (ReferenceEquals(slot.Items[i], owner))
                {
                    slot.Items[i] = null;
                    slot.Live--;
                    break;
                }
            }

            // Đang dispatch thì giữ nguyên vị trí; dọn sau khi dispatch xong.
            if (s_dispatchDepth != 0)
                return;

            Compact(slot);

            if (slot.Live == 0)
            {
                s_map.Remove(id);
                s_free.Push(slot);
            }
        }

        static void Compact(OptSlot slot)
        {
            if (slot.Live == slot.Count)
                return;

            int write = 0;
            for (int read = 0; read < slot.Count; read++)
            {
                MonoBehaviour item = slot.Items[read];
                if (item != null)
                    slot.Items[write++] = item;
            }

            for (int i = write; i < slot.Count; i++)
                slot.Items[i] = null;

            slot.Count = write;
        }

        internal static void Clear()
        {
            s_map.Clear();
            s_dispatchDepth = 0;
        }
    }
}
