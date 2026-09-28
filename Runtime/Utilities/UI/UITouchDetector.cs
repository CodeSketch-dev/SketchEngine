using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SketchEngine.Utilities.UI
{
    /// <summary>
    /// UITouchDetector
    /// 
    /// Purpose:
    /// - Unified pointer/touch event detector
    /// - Supports mouse + single-touch
    /// - Prevents multi-touch conflict
    ///
    /// Typical usage:
    /// - Custom UI buttons
    /// - Virtual joystick
    /// - Drag UI element
    /// </summary>
    public sealed class UITouchDetector :
        MonoBehaviour,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        /// <summary>
        /// Global active pointer id (shared across all detectors).
        /// </summary>
        static int s_activePointerId = int.MinValue;

        bool _isDragging;

        // =====================================================
        // EVENTS
        // =====================================================

        public event Action<PointerEventData> eventPointerClick;
        public event Action<PointerEventData> eventPointerDown;
        public event Action<PointerEventData> eventPointerUp;

        public event Action<PointerEventData> eventPointerEnter;
        public event Action<PointerEventData> eventPointerExit;

        public event Action<PointerEventData> eventBeginDrag;
        public event Action<PointerEventData> eventDrag;
        public event Action<PointerEventData> eventEndDrag;

        // =====================================================
        // POINTER
        // =====================================================

        public void OnPointerDown(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            // Already captured by another pointer
            if (s_activePointerId != int.MinValue)
                return;

            s_activePointerId = eventData.pointerId;
#endif
            _isDragging = false;
            eventPointerDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            // Ignore if not owner
            if (eventData.pointerId != s_activePointerId)
                return;

            s_activePointerId = int.MinValue;
#endif
            _isDragging = false;
            eventPointerUp?.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging)
                return;

            eventPointerClick?.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            eventPointerEnter?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            eventPointerExit?.Invoke(eventData);
        }

        // =====================================================
        // DRAG
        // =====================================================

        public void OnBeginDrag(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            if (eventData.pointerId != s_activePointerId)
                return;
#endif
            _isDragging = true;
            eventBeginDrag?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            if (eventData.pointerId != s_activePointerId)
                return;
#endif
            eventDrag?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            if (eventData.pointerId != s_activePointerId)
                return;
#endif
            _isDragging = false;
            eventEndDrag?.Invoke(eventData);
        }
    }
}
