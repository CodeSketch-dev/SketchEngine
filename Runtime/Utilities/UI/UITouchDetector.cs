using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SketchEngine.Utilities.UI
{
    /// <summary>
    /// UITouchDetector
    /// 
    /// Mục đích:
    /// - Phát hiện thống nhất các sự kiện con trỏ và cảm ứng
    /// - Hỗ trợ chuột và cảm ứng một điểm chạm
    /// - Ngăn xung đột khi có nhiều điểm chạm
    ///
    /// Trường hợp sử dụng phổ biến:
    /// - Nút giao diện tùy chỉnh
    /// - Cần điều khiển ảo
    /// - Kéo phần tử giao diện
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
        /// ID của con trỏ đang hoạt động, dùng chung cho tất cả bộ phát hiện.
        /// </summary>
        static int ACTIVE_POINTER_ID = int.MinValue;

        bool _isDragging;

        // =====================================================
        // SỰ KIỆN
        // =====================================================

        public event Action<PointerEventData> EventPointerClick;
        public event Action<PointerEventData> EventPointerDown;
        public event Action<PointerEventData> EventPointerUp;

        public event Action<PointerEventData> EventPointerEnter;
        public event Action<PointerEventData> EventPointerExit;

        public event Action<PointerEventData> EventBeginDrag;
        public event Action<PointerEventData> EventDrag;
        public event Action<PointerEventData> EventEndDrag;

        // =====================================================
        // CON TRỎ
        // =====================================================

        public void OnPointerDown(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            // Một con trỏ khác đã chiếm quyền xử lý
            if (ACTIVE_POINTER_ID != int.MinValue)
                return;

            ACTIVE_POINTER_ID = eventData.pointerId;
#endif
            _isDragging = false;
            EventPointerDown?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            // Bỏ qua nếu con trỏ không có quyền xử lý
            if (eventData.pointerId != ACTIVE_POINTER_ID)
                return;

            ACTIVE_POINTER_ID = int.MinValue;
#endif
            _isDragging = false;
            EventPointerUp?.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isDragging)
                return;

            EventPointerClick?.Invoke(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            EventPointerEnter?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            EventPointerExit?.Invoke(eventData);
        }

        // =====================================================
        // KÉO
        // =====================================================

        public void OnBeginDrag(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            if (eventData.pointerId != ACTIVE_POINTER_ID)
                return;
#endif
            _isDragging = true;
            EventBeginDrag?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            if (eventData.pointerId != ACTIVE_POINTER_ID)
                return;
#endif
            EventDrag?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
#if !UNITY_EDITOR
            if (eventData.pointerId != ACTIVE_POINTER_ID)
                return;
#endif
            _isDragging = false;
            EventEndDrag?.Invoke(eventData);
        }
    }
}
