using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    /// <summary>
    /// Các hàm mở rộng dành riêng cho Camera Orthographic.
    /// Dễ dàng điều chỉnh và truy xuất kích thước vùng hiển thị.
    /// </summary>
    public static class ExtensionsCameraOrthographic
    {
        /// <summary>
        /// Thiết lập camera để hiển thị vùng có chiều rộng mong muốn.
        /// </summary>
        public static void SetWidth(this Camera camera, float width)
        {
            camera.orthographicSize = camera.GetOrthoSizeFromWidth(width);
        }

        /// <summary>
        /// Thiết lập camera để hiển thị vùng có chiều cao mong muốn.
        /// </summary>
        public static void SetHeight(this Camera camera, float height)
        {
            camera.orthographicSize = camera.GetOrthoSizeFromHeight(height);
        }

        /// <summary>
        /// Tính orthographicSize dựa trên chiều rộng mong muốn.
        /// </summary>
        public static float GetOrthoSizeFromWidth(this Camera camera, float width)
        {
            return width / camera.aspect * 0.5f;
        }

        /// <summary>
        /// Tính orthographicSize dựa trên chiều cao mong muốn.
        /// </summary>
        public static float GetOrthoSizeFromHeight(this Camera camera, float height)
        {
            return height * 0.5f;
        }

        /// <summary>
        /// Lấy chiều cao hiện tại vùng hiển thị của camera.
        /// </summary>
        public static float GetHeight(this Camera camera)
        {
            return camera.orthographicSize * 2f;
        }

        /// <summary>
        /// Lấy chiều rộng hiện tại vùng hiển thị của camera.
        /// </summary>
        public static float GetWidth(this Camera camera)
        {
            return camera.GetHeight() * camera.aspect;
        }

        /// <summary>
        /// Tọa độ Y ở mép trên của màn hình camera (world space).
        /// </summary>
        public static float Top(this Camera camera)
        {
            return camera.transform.position.y + camera.GetHeight() * 0.5f;
        }

        /// <summary>
        /// Tọa độ Y ở mép dưới của màn hình camera (world space).
        /// </summary>
        public static float Bottom(this Camera camera)
        {
            return camera.transform.position.y - camera.GetHeight() * 0.5f;
        }

        /// <summary>
        /// Tọa độ X ở mép phải của màn hình camera (world space).
        /// </summary>
        public static float Right(this Camera camera)
        {
            return camera.transform.position.x + camera.GetWidth() * 0.5f;
        }

        /// <summary>
        /// Tọa độ X ở mép trái của màn hình camera (world space).
        /// </summary>
        public static float Left(this Camera camera)
        {
            return camera.transform.position.x - camera.GetWidth() * 0.5f;
        }
    }
}
