using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    /// <summary>
    /// Các hàm mở rộng tiện ích cho RectTransform
    /// </summary>
    public static class ExtensionsRectTransform
    {
        // Mảng dùng chung để lấy góc, tránh cấp phát mỗi lần gọi. Chỉ dùng trên main thread.
        static readonly Vector3[] s_corners = new Vector3[4];

        /// <summary>Set chiều rộng của RectTransform theo Anchor hiện tại.</summary>
        public static void SetWidth(this RectTransform rectTransform, float width)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        /// <summary>Set chiều cao của RectTransform theo Anchor hiện tại.</summary>
        public static void SetHeight(this RectTransform rectTransform, float height)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        /// <summary>Set tọa độ X của anchoredPosition.</summary>
        public static void SetAnchoredPositionX(this RectTransform rectTransform, float x)
        {
            rectTransform.anchoredPosition = new Vector2(x, rectTransform.anchoredPosition.y);
        }

        /// <summary>Set tọa độ Y của anchoredPosition.</summary>
        public static void SetAnchoredPositionY(this RectTransform rectTransform, float y)
        {
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, y);
        }

        /// <summary>Dịch anchoredPosition theo trục X.</summary>
        public static void TranslateX(this RectTransform rectTransform, float x)
        {
            rectTransform.anchoredPosition += new Vector2(x, 0);
        }

        /// <summary>Dịch anchoredPosition theo trục Y.</summary>
        public static void TranslateY(this RectTransform rectTransform, float y)
        {
            rectTransform.anchoredPosition += new Vector2(0, y);
        }

        /// <summary>Dịch anchoredPosition theo cả hai trục X và Y.</summary>
        public static void TranslateXY(this RectTransform rectTransform, float x, float y)
        {
            rectTransform.anchoredPosition += new Vector2(x, y);
        }

        /// <summary>Kéo full kích thước theo cha (Stretch full parent).</summary>
        public static void StretchByParent(this RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>Trả về tỷ lệ pixel trên mỗi đơn vị thế giới (pixel per unit).</summary>
        public static float GetPixelPerUnit(this RectTransform rectTransform)
        {
            Vector3[] corners = s_corners;
            rectTransform.GetWorldCorners(corners);
            return rectTransform.rect.width / (corners[2].x - corners[0].x);
        }

        /// <summary>Trả về tỷ lệ đơn vị thế giới trên mỗi pixel.</summary>
        public static float GetUnitPerPixel(this RectTransform rectTransform)
        {
            Vector3[] corners = s_corners;
            rectTransform.GetWorldCorners(corners);
            return (corners[2].x - corners[0].x) / rectTransform.rect.width;
        }

        /// <summary>Trả về vị trí giữa RectTransform trong không gian thế giới.</summary>
        public static Vector2 ToScreen(this RectTransform rectTransform)
        {
            return rectTransform.TransformPoint(Vector3.zero);
        }

        /// <summary>
        /// Thiết lập vị trí và kích thước RectTransform theo 4 điểm màn hình.
        /// screenCorners phải có đúng 4 phần tử theo thứ tự: bottom-left, top-left, top-right, bottom-right.
        /// </summary>
        public static void SetWorldCorners(this RectTransform rectTransform, Vector2[] screenCorners)
        {
            if (screenCorners == null || screenCorners.Length != 4) return;

            RectTransform parent = rectTransform.parent as RectTransform;
            if (parent == null) return;

            Vector2[] localCorners = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenCorners[i], null, out localCorners[i]);
            }

            Vector2 center = (localCorners[0] + localCorners[2]) * 0.5f;
            rectTransform.anchoredPosition = center;

            float width = Mathf.Abs(localCorners[3].x - localCorners[0].x);
            float height = Mathf.Abs(localCorners[1].y - localCorners[0].y);

            rectTransform.SetWidth(width);
            rectTransform.SetHeight(height);
        }

        /// <summary>Reset anchor, pivot, size, vị trí về mặc định giữa (giữa màn hình, 100x100).</summary>
        public static void ResetLayout(this RectTransform rectTransform)
        {
            rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(100f, 100f);
        }

        /// <summary>Set anchor min/max và pivot cùng lúc.</summary>
        public static void SetAnchorAndPivot(this RectTransform rectTransform, Vector2 anchor)
        {
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = anchor;
        }

        /// <summary>Trả về vị trí góc dưới trái trong thế giới.</summary>
        public static Vector3 GetWorldBottomLeft(this RectTransform rectTransform)
        {
            Vector3[] corners = s_corners;
            rectTransform.GetWorldCorners(corners);
            return corners[0];
        }

        /// <summary>Trả về vị trí góc trên phải trong thế giới.</summary>
        public static Vector3 GetWorldTopRight(this RectTransform rectTransform)
        {
            Vector3[] corners = s_corners;
            rectTransform.GetWorldCorners(corners);
            return corners[2];
        }

        /// <summary>
        /// Căn chỉnh RectTransform để bao phủ đúng Bounds của một đối tượng 3D.
        /// </summary>
        /// <param name="rectTransform">RectTransform cần căn chỉnh.</param>
        /// <param name="bounds">Bounds trong thế giới cần fit vào RectTransform.</param>
        /// <param name="camera">Camera sử dụng để chuyển đổi (nếu null sẽ tự tìm camera chính).</param>
        public static void FitToWorldBounds(this RectTransform rectTransform, Bounds bounds, Camera camera = null)
        {
            if (camera == null)
                camera = Camera.main;

            // Dùng Vector3 để giữ đúng Z khi chiếu lên màn hình (Vector2 sẽ làm Z = 0 sai).
            // Các góc chiếu ở cùng độ sâu (tâm bounds) để hình chữ nhật trên màn hình đúng.
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            float z = bounds.center.z;

            var screenCorners = new Vector2[4];
            screenCorners[0] = camera.WorldToScreenPoint(new Vector3(min.x, min.y, z));
            screenCorners[1] = camera.WorldToScreenPoint(new Vector3(min.x, max.y, z));
            screenCorners[2] = camera.WorldToScreenPoint(new Vector3(max.x, max.y, z));
            screenCorners[3] = camera.WorldToScreenPoint(new Vector3(max.x, min.y, z));

            rectTransform.SetWorldCorners(screenCorners);
        }
    }
}
