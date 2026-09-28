using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    /// <summary>
    /// Extension methods tiện ích cho Renderer (MeshRenderer, SkinnedMeshRenderer, v.v.)
    /// </summary>
    public static class ExtensionsRenderer
    {
        /// <summary>
        /// Đặt kích thước thực tế của renderer bằng cách scale transform
        /// </summary>
        public static void SetSize(this Renderer renderer, Vector3 targetSize)
        {
            Vector3 currentSize = renderer.bounds.size;
            if (currentSize == Vector3.zero) return;

            Vector3 scaleFactor = new Vector3(
                targetSize.x / currentSize.x,
                targetSize.y / currentSize.y,
                targetSize.z / currentSize.z
            );

            renderer.transform.localScale = Vector3.Scale(renderer.transform.localScale, scaleFactor);
        }

        /// <summary>
        /// Đặt kích thước với parent (pivot) làm gốc scale
        /// </summary>
        public static void SetSizeWithParentPivot(this Renderer renderer, Vector3 targetSize)
        {
            Transform parentTransform = renderer.transform.parent;
            if (parentTransform == null)
            {
                Debug.LogError("Object has no parent. This function requires a parent to scale.");
                return;
            }

            Vector3 currentSize = renderer.bounds.size;
            if (currentSize == Vector3.zero) return;

            Vector3 scaleFactor = new Vector3(
                targetSize.x / currentSize.x,
                targetSize.y / currentSize.y,
                targetSize.z / currentSize.z
            );

            parentTransform.localScale = Vector3.Scale(parentTransform.localScale, scaleFactor);
        }

        /// <summary>
        /// Đặt kích thước thực tế theo từng chiều
        /// </summary>
        public static void SetSize(this Renderer renderer, float x, float y, float z)
        {
            SetSize(renderer, new Vector3(x, y, z));
        }

        /// <summary>
        /// Đặt kích thước chỉ theo trục X và Z (giữ nguyên Y)
        /// </summary>
        public static void SetSizeXZ(this Renderer renderer, float x, float z)
        {
            Vector3 currentSize = renderer.bounds.size;
            if (currentSize == Vector3.zero) return;

            Vector3 scaleFactor = new Vector3(
                x / currentSize.x,
                1f,
                z / currentSize.z
            );

            renderer.transform.localScale = Vector3.Scale(renderer.transform.localScale, scaleFactor);
        }

        /// <summary>
        /// Lấy kích thước thực tế trong thế giới (bounds.size)
        /// </summary>
        public static Vector3 GetWorldSize(this Renderer renderer)
        {
            return renderer.bounds.size;
        }

        /// <summary>
        /// Lấy center vị trí trung tâm của Renderer trong thế giới
        /// </summary>
        public static Vector3 GetWorldCenter(this Renderer renderer)
        {
            return renderer.bounds.center;
        }

        /// <summary>
        /// Scale renderer sao cho vừa trong box mục tiêu, giữ tỷ lệ gốc (fit vào kích thước nhỏ nhất)
        /// </summary>
        public static void FitInsideBox(this Renderer renderer, Vector3 maxBoxSize)
        {
            Vector3 currentSize = renderer.bounds.size;
            if (currentSize == Vector3.zero) return;

            float scaleRatio = Mathf.Min(
                maxBoxSize.x / currentSize.x,
                maxBoxSize.y / currentSize.y,
                maxBoxSize.z / currentSize.z
            );

            renderer.transform.localScale *= scaleRatio;
        }

        /// <summary>
        /// Reset scale về Vector3.one
        /// </summary>
        public static void ResetSize(this Renderer renderer)
        {
            renderer.transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Scale đến kích thước cụ thể và giữ nguyên vị trí center hiện tại
        /// </summary>
        public static void SetSizeKeepCenter(this Renderer renderer, Vector3 targetSize)
        {
            Vector3 oldCenter = renderer.bounds.center;

            renderer.SetSize(targetSize);

            Vector3 newCenter = renderer.bounds.center;
            Vector3 offset = oldCenter - newCenter;

            renderer.transform.position += offset;
        }
    }
}
