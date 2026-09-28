using System;
using SketchEngine.Mono;
using UnityEngine.EventSystems;

namespace SketchEngine.Utilities.UI
{
    public class UIPointerClick : MonoBase, IPointerDownHandler, IPointerUpHandler
    {
        public event Action EventDown;
        public event Action EventUp;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            EventDown?.Invoke();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            EventUp?.Invoke();
        }
    }
}
