using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    /// <summary>
    /// Extension methods tiện ích cho Rigidbody2D
    /// </summary>
    public static class ExtensionsRigidbody2D
    {
        /// <summary>
        /// Mô phỏng lực nổ (explosion force) áp dụng lên Rigidbody2D tại vị trí nhất định
        /// </summary>
        /// <param name="rb">Rigidbody2D</param>
        /// <param name="explosionForce">Lực tác động tối đa</param>
        /// <param name="explosionPosition">Vị trí tâm vụ nổ</param>
        /// <param name="explosionRadius">Bán kính ảnh hưởng</param>
        /// <param name="upwardsModifier">Tăng lực theo chiều Y</param>
        /// <param name="mode">Kiểu lực (Force hoặc Impulse)</param>
        public static void AddExplosionForce(this Rigidbody2D rb, float explosionForce, Vector2 explosionPosition, float explosionRadius, float upwardsModifier = 0.0f, ForceMode2D mode = ForceMode2D.Force)
        {
            Vector2 explosionDir = rb.position - explosionPosition;
            float explosionDistance = explosionDir.magnitude;

            // Nếu nằm ngoài vùng nổ thì bỏ qua
            if (explosionDistance > explosionRadius)
                return;

            // Chuẩn hóa vector
            if (explosionDistance > 0.01f)
            {
                explosionDir /= explosionDistance; // normalize
            }
            else
            {
                explosionDir = Vector2.up; // fallback hướng ngẫu nhiên nếu quá gần
            }

            if (upwardsModifier != 0)
                explosionDir.y += upwardsModifier;

            explosionDir.Normalize();

            float force = Mathf.Lerp(explosionForce, 0f, explosionDistance / explosionRadius);
            rb.AddForce(force * explosionDir, mode);
        }

        /// <summary>
        /// Đẩy Rigidbody2D theo hướng từ attacker đến receiver (thường dùng để knockback)
        /// </summary>
        public static void ApplyKnockbackForce(this Rigidbody2D rb, Vector2 fromPosition, float force, float upwardModifier = 0f, ForceMode2D mode = ForceMode2D.Impulse)
        {
            Vector2 dir = rb.position - fromPosition;
            if (dir.sqrMagnitude < 0.01f)
                dir = Vector2.up;

            dir.Normalize();
            dir.y += upwardModifier;
            dir.Normalize();

            rb.AddForce(dir * force, mode);
        }

        /// <summary>
        /// Ngừng mọi chuyển động vật lý (velocity và angular)
        /// </summary>
        public static void StopMotion(this Rigidbody2D rb)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        /// <summary>
        /// Giới hạn tốc độ tối đa của Rigidbody2D
        /// </summary>
        public static void LimitMaxSpeed(this Rigidbody2D rb, float maxSpeed)
        {
            if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
        }
    }
}
