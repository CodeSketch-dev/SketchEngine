using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace SketchEngine.Core.Extensions
{
    public static class ExtensionsScrollRect
    {
        public static void ScrollTo(this ScrollRect scrollRect, Transform target, bool isVertical = true)
        {
            if (isVertical)
                scrollRect.normalizedPosition = new Vector2(0f, 1f - (scrollRect.content.rect.height / 2f - target.localPosition.y) / scrollRect.content.rect.height);
            else
                scrollRect.normalizedPosition = new Vector2(1f - (scrollRect.content.rect.width / 2f - target.localPosition.x) / scrollRect.content.rect.width, 0f);
        }
        
        /// Cuộn mượt để target nằm CHÍNH GIỮA viewport (pivot/anchor nào cũng đúng)
        public static void ScrollToCenter(this ScrollRect sr, RectTransform target, float duration = 0.25f) {
            if (!sr || !sr.content || !target) return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(sr.content);

            var content  = sr.content;
            var viewport = sr.viewport ? sr.viewport : (RectTransform)sr.transform;

            float contentW = content.rect.width;
            float contentH = content.rect.height;
            float viewW    = viewport.rect.width;
            float viewH    = viewport.rect.height;

            bool canScrollH = sr.horizontal && contentW > viewW + 0.5f;
            bool canScrollV = sr.vertical   && contentH > viewH + 0.5f;

            // Tâm target trong LOCAL của content (ổn định cho Grid nhiều cột)
            var b = RectTransformUtility.CalculateRelativeRectTransformBounds(content, target);
            Vector3 targetCenter = b.center;

            Rect cr = content.rect;
            float leftX   = cr.xMin;
            float rightX  = cr.xMax;
            float bottomY = cr.yMin;
            float topY    = cr.yMax;

            // --- Horizontal: đưa tâm target trùng tâm viewport ---
            float endX = sr.normalizedPosition.x;
            if (canScrollH) {
                float xLeftDesired = targetCenter.x - (viewW * 0.5f);             // mép trái viewport mong muốn
                xLeftDesired = Mathf.Clamp(xLeftDesired, leftX, rightX - viewW);  // biên hợp lệ của mép trái
                endX = Mathf.InverseLerp(leftX, rightX - viewW, xLeftDesired);    // 0=left, 1=right
            }

            // --- Vertical: tương tự (Unity: 1=top, 0=bottom) ---
            float endY = sr.normalizedPosition.y;
            if (canScrollV) {
                float yTopMin    = topY - (contentH - viewH);                     // = bottomY + viewH
                float yTopDesired = targetCenter.y + (viewH * 0.5f);              // mép trên viewport mong muốn
                yTopDesired = Mathf.Clamp(yTopDesired, yTopMin, topY);
                endY = Mathf.InverseLerp(yTopMin, topY, yTopDesired);             // 1=top, 0=bottom
            }

            // Tween normalizedPosition (PrimeTween dùng Custom)
            var start = sr.normalizedPosition;
            Sequence.Create()
                .Group(Tween.Custom(start.x, endX, duration, v => {
                    var np = sr.normalizedPosition; np.x = v; sr.normalizedPosition = np;
                }, Ease.OutCubic))
                .Group(Tween.Custom(start.y, endY, duration, v => {
                    var np = sr.normalizedPosition; np.y = v; sr.normalizedPosition = np;
                }, Ease.OutCubic));
        }
    }
}
