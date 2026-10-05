using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    /// <summary>
    /// Extension methods tiện ích cho Renderer (MeshRenderer, SpriteRenderer, SkinnedMeshRenderer, v.v.)
    /// </summary>
    public static class ExtensionsRenderer
    {
        const float Epsilon = 0.0001f;

        /// <summary>Đặt kích thước thực tế (world bounds) của renderer bằng cách scale transform.</summary>
        public static void SetSize(this Renderer renderer, Vector3 targetSize)
        {
            Vector3 current = renderer.bounds.size;
            renderer.transform.localScale = Vector3.Scale(renderer.transform.localScale, ScaleFactor(targetSize, current));
        }

        public static void SetSize(this Renderer renderer, float x, float y, float z)
        {
            SetSize(renderer, new Vector3(x, y, z));
        }

        /// <summary>Đặt kích thước theo X và Z, giữ nguyên Y.</summary>
        public static void SetSizeXZ(this Renderer renderer, float x, float z)
        {
            Vector3 current = renderer.bounds.size;
            Vector3 factor = new Vector3(AxisFactor(x, current.x), 1f, AxisFactor(z, current.z));
            renderer.transform.localScale = Vector3.Scale(renderer.transform.localScale, factor);
        }

        /// <summary>Đặt kích thước với parent làm gốc scale.</summary>
        public static void SetSizeWithParentPivot(this Renderer renderer, Vector3 targetSize)
        {
            Transform parent = renderer.transform.parent;
            if (parent == null)
            {
                Debug.LogError("Object has no parent. This function requires a parent to scale.");
                return;
            }

            Vector3 current = renderer.bounds.size;
            parent.localScale = Vector3.Scale(parent.localScale, ScaleFactor(targetSize, current));
        }

        /// <summary>Đặt kích thước và giữ nguyên vị trí center hiện tại.</summary>
        public static void SetSizeKeepCenter(this Renderer renderer, Vector3 targetSize)
        {
            Vector3 oldCenter = renderer.bounds.center;

            renderer.SetSize(targetSize);

            renderer.transform.position += oldCenter - renderer.bounds.center;
        }

        /// <summary>Scale vừa trong box mục tiêu, giữ tỷ lệ gốc.</summary>
        public static void FitInsideBox(this Renderer renderer, Vector3 maxBoxSize)
        {
            Vector3 current = renderer.bounds.size;

            float ratio = float.MaxValue;
            if (current.x > Epsilon) ratio = Mathf.Min(ratio, maxBoxSize.x / current.x);
            if (current.y > Epsilon) ratio = Mathf.Min(ratio, maxBoxSize.y / current.y);
            if (current.z > Epsilon) ratio = Mathf.Min(ratio, maxBoxSize.z / current.z);

            if (ratio == float.MaxValue) return;

            renderer.transform.localScale *= ratio;
        }

        public static Vector3 GetWorldSize(this Renderer renderer) => renderer.bounds.size;

        public static Vector3 GetWorldCenter(this Renderer renderer) => renderer.bounds.center;

        public static void ResetSize(this Renderer renderer) => renderer.transform.localScale = Vector3.one;

        // Trục có kích thước ~0 (vd sprite phẳng trên Z) thì giữ hệ số 1 thay vì chia cho 0.
        static Vector3 ScaleFactor(Vector3 target, Vector3 current)
        {
            return new Vector3(
                AxisFactor(target.x, current.x),
                AxisFactor(target.y, current.y),
                AxisFactor(target.z, current.z));
        }

        static float AxisFactor(float target, float current)
        {
            return current > Epsilon ? target / current : 1f;
        }
    }
}
