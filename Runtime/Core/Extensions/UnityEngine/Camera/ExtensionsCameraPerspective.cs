using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    /// <summary>
    /// Các hàm mở rộng cho Camera Perspective để tính toán kích thước vùng nhìn thấy (view size)
    /// hoặc FOV cần thiết để hiển thị một khu vực ở khoảng cách nhất định.
    /// </summary>
    public static class ExtensionsCameraPerspective
    {
        /// <summary>
        /// Tính kích thước vùng nhìn thấy (width và height) của camera ở một khoảng cách cụ thể.
        /// </summary>
        /// <param name="camera">Camera Perspective</param>
        /// <param name="distance">Khoảng cách dọc theo forward direction</param>
        /// <returns>Kích thước (width, height) vùng hiển thị tại khoảng cách đó</returns>
        public static Vector2 SizeAtDistance(this Camera camera, float distance)
        {
            float height = 2f * Mathf.Abs(distance) * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * camera.aspect;
            return new Vector2(width, height);
        }

        /// <summary>
        /// Lấy chiều rộng hiển thị của camera tại khoảng cách nhất định.
        /// </summary>
        public static float SizeWidthAtDistance(this Camera camera, float distance)
        {
            return SizeAtDistance(camera, distance).x;
        }

        /// <summary>
        /// Lấy chiều cao hiển thị của camera tại khoảng cách nhất định.
        /// </summary>
        public static float SizeHeightAtDistance(this Camera camera, float distance)
        {
            return SizeAtDistance(camera, distance).y;
        }

        /// <summary>
        /// Tính FOV cần thiết để camera hiển thị vừa khít một vùng (rectSize) tại khoảng cách nhất định.
        /// Ưu tiên chiều rộng, sau đó fallback qua chiều cao nếu không đủ.
        /// </summary>
        /// <param name="rectSize">Kích thước vùng cần fit (width, height)</param>
        /// <param name="distance">Khoảng cách tới camera</param>
        /// <returns>Field of View (degree) phù hợp</returns>
        public static float FOVFitRectAtDistance(this Camera camera, Vector2 rectSize, float distance)
        {
            float fieldOfView = rectSize.x >= rectSize.y
                ? FOVFitWidthAtDistance(camera, rectSize.x, distance)
                : FOVFitHeightAtDistance(camera, rectSize.y, distance);

            // Kiểm tra nếu FOV trên vẫn không đủ để bao toàn bộ chiều cao → fallback
            float visibleHeight = 2f * Mathf.Abs(distance) * Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);
            if (rectSize.y > visibleHeight)
            {
                fieldOfView = FOVFitHeightAtDistance(camera, rectSize.y, distance);
            }

            return fieldOfView;
        }

        /// <summary>
        /// Tính Field of View (vertical FOV) cần thiết để nhìn thấy toàn bộ chiều cao ở khoảng cách cho trước.
        /// </summary>
        public static float FOVFitHeightAtDistance(this Camera camera, float height, float distance)
        {
            return 2f * Mathf.Atan(height / (2f * Mathf.Abs(distance))) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Tính Field of View (vertical FOV) cần thiết để nhìn thấy toàn bộ chiều rộng ở khoảng cách cho trước.
        /// Sử dụng aspect để quy đổi sang chiều cao.
        /// </summary>
        public static float FOVFitWidthAtDistance(this Camera camera, float width, float distance)
        {
            float height = width / camera.aspect;
            return FOVFitHeightAtDistance(camera, height, distance);
        }

        /// <summary>
        /// Trả về vị trí cách camera một khoảng `distance` theo hướng nhìn (forward)
        /// </summary>
        /// <param name="camera">Camera nguồn</param>
        /// <param name="distance">Khoảng cách</param>
        /// <returns>Vị trí trong thế giới</returns>
        public static Vector3 GetPointInViewDirection(this Camera camera, float distance)
        {
            return camera.transform.position + camera.transform.forward * distance;
        }
    }
}
