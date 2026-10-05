using System;
using SketchEngine.Mono;
using UnityEngine.EventSystems;

namespace SketchEngine.Utilities.UI
{
    public class UIPointerDrag : MonoCached, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> EventDragBegin;
        public Action<PointerEventData> EventDrag;
        public Action<PointerEventData> EventDragEnd;

        // ✅ Dùng public virtual thay vì explicit interface → có thể override
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            EventDragBegin?.Invoke(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            EventDrag?.Invoke(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            EventDragEnd?.Invoke(eventData);
        }
    }
}