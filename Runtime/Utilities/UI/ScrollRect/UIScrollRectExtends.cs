using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SketchEngine.Utilities.UI
{
    public class UIScrollRectExtends : ScrollRect
    {
        public Action<PointerEventData> EventBeginDrag;
        public Action<PointerEventData> EventDrag;
        public Action<PointerEventData> EventEndDrag;
        
        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            
            EventBeginDrag?.Invoke(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            
            EventDrag?.Invoke(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            
            EventEndDrag?.Invoke(eventData);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            Restore();
        }
        
        void Restore()
        {
            if (vertical)
            {
                verticalNormalizedPosition = 1f;
            }
            else
            {
                verticalNormalizedPosition = 0f;
            }
        }
    }
}
