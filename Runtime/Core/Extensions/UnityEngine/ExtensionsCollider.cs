using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    public static class ExtensionsCollider
    {
        /// <summary>
        /// Clone các thuộc tính cơ bản giữa 2 collider cùng loại (Box, Sphere, Capsule,...).
        /// Tự động kiểm tra kiểu và sao chép chính xác các thuộc tính tương ứng.
        /// </summary>
        public static void CloneFrom(this Collider target, Collider source)
        {
            if (target == null || source == null)
            {
                Debug.LogWarning("Source hoặc Target Collider là null.");
                return;
            }

            if (target.GetType() != source.GetType())
            {
                Debug.LogWarning($"Không thể clone từ {source.GetType().Name} sang {target.GetType().Name}.");
                return;
            }

            target.isTrigger = source.isTrigger;
            // sharedMaterial: không tạo bản sao material (material sẽ tạo instance mới và rò rỉ).
            target.sharedMaterial = source.sharedMaterial;

            switch (source)
            {
                case BoxCollider srcBox when target is BoxCollider tgtBox:
                    tgtBox.center = srcBox.center;
                    tgtBox.size = srcBox.size;
                    break;

                case SphereCollider srcSphere when target is SphereCollider tgtSphere:
                    tgtSphere.center = srcSphere.center;
                    tgtSphere.radius = srcSphere.radius;
                    break;

                case CapsuleCollider srcCapsule when target is CapsuleCollider tgtCapsule:
                    tgtCapsule.center = srcCapsule.center;
                    tgtCapsule.radius = srcCapsule.radius;
                    tgtCapsule.height = srcCapsule.height;
                    tgtCapsule.direction = srcCapsule.direction;
                    break;

                case MeshCollider srcMesh when target is MeshCollider tgtMesh:
                    tgtMesh.sharedMesh = srcMesh.sharedMesh;
                    tgtMesh.convex = srcMesh.convex;
                    break;

                default:
                    Debug.LogWarning($"Chưa hỗ trợ clone cho collider kiểu {source.GetType().Name}");
                    break;
            }
        }
    }
}
