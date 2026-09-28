using System;
using System.Collections.Generic;
using UnityEngine;

namespace SketchEngine.Utilities.Paint
{
    /// <summary>
    /// MeshPaintMath
    /// 
    /// Purpose:
    /// - Geometry helpers for mesh painting / decal / hit effect
    /// - NOT a physics or collision replacement
    ///
    /// Typical usage:
    /// - Raycast hit → find triangle → check point
    /// - Calculate UV at hit position
    /// - Clamp / project point into triangle
    /// </summary>
    public static class MeshPaintMath
    {
        /// <summary>
        /// Numerical tolerance for float comparisons.
        /// </summary>
        const float TOLERANCE = 1e-4f;

        // =====================================================
        // PLANE
        // =====================================================

        /// <summary>
        /// Check if point P lies on the plane defined by triangle (t1, t2, t3).
        /// 
        /// Usage:
        /// - Use before triangle inside test
        /// - Raycast hit usually already lies on plane
        /// </summary>
        public static bool IsPointOnPlane(
            Vector3 p,
            Vector3 t1,
            Vector3 t2,
            Vector3 t3
        )
        {
            Vector3 normal = Vector3.Cross(t2 - t1, t3 - t1);
            float distance = Vector3.Dot(normal.normalized, p - t1);
            return Mathf.Abs(distance) < TOLERANCE;
        }

        // =====================================================
        // TRIANGLE TEST (BARYCENTRIC)
        // =====================================================

        /// <summary>
        /// Check if point P is inside triangle (t1, t2, t3).
        /// 
        /// REQUIRE:
        /// - Point must already lie on the triangle plane
        ///
        /// Fast, stable, and standard barycentric test.
        /// </summary>
        public static bool IsPointInTriangle(
            Vector3 p,
            Vector3 t1,
            Vector3 t2,
            Vector3 t3
        )
        {
            Vector3 v0 = t2 - t1;
            Vector3 v1 = t3 - t1;
            Vector3 v2 = p - t1;

            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);

            float denom = d00 * d11 - d01 * d01;
            if (Mathf.Abs(denom) < TOLERANCE)
                return false;

            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1f - v - w;

            return u >= -TOLERANCE && v >= -TOLERANCE && w >= -TOLERANCE;
        }

        // =====================================================
        // EDGE
        // =====================================================

        /// <summary>
        /// Check if point P lies on edge (a → b).
        /// 
        /// Used for:
        /// - Brush snapping
        /// - Edge detection in paint tools
        ///
        /// NOT exact for physics.
        /// </summary>
        public static bool IsPointOnEdge(
            Vector3 p,
            Vector3 a,
            Vector3 b
        )
        {
            Vector3 ab = b - a;
            Vector3 ap = p - a;

            float t = Vector3.Dot(ap, ab) / ab.sqrMagnitude;
            if (t < 0f || t > 1f)
                return false;

            Vector3 closest = a + t * ab;
            return (p - closest).sqrMagnitude < TOLERANCE * TOLERANCE;
        }

        /// <summary>
        /// Check if point lies on any edge of a triangle.
        /// </summary>
        public static bool IsPointOnTriangleEdge(
            Vector3 p,
            Vector3 t1,
            Vector3 t2,
            Vector3 t3
        )
        {
            return
                IsPointOnEdge(p, t1, t2) ||
                IsPointOnEdge(p, t2, t3) ||
                IsPointOnEdge(p, t3, t1);
        }

        // =====================================================
        // UV CALCULATION (CORE FOR PAINT / DECAL)
        // =====================================================

        /// <summary>
        /// Calculate UV coordinate at point P inside triangle.
        /// 
        /// REQUIRE:
        /// - P must be inside triangle
        ///
        /// Typical usage:
        /// RaycastHit hit
        /// → mesh.triangles
        /// → mesh.uv
        /// → interpolate UV at hit.point
        /// </summary>
        public static Vector2 CalculateUV(
            Vector3 p,
            Vector3 t1, Vector2 uv1,
            Vector3 t2, Vector2 uv2,
            Vector3 t3, Vector2 uv3
        )
        {
            Vector3 v0 = t2 - t1;
            Vector3 v1 = t3 - t1;
            Vector3 v2 = p - t1;

            float d00 = Vector3.Dot(v0, v0);
            float d01 = Vector3.Dot(v0, v1);
            float d11 = Vector3.Dot(v1, v1);
            float d20 = Vector3.Dot(v2, v0);
            float d21 = Vector3.Dot(v2, v1);

            float denom = d00 * d11 - d01 * d01;

            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1f - v - w;

            return uv1 * u + uv2 * v + uv3 * w;
        }

        // =====================================================
        // TRIANGLE SELECTION (HEURISTIC)
        // =====================================================

        /// <summary>
        /// Find triangle whose centroid is closest to point P.
        /// 
        /// Heuristic only:
        /// - Good for paint / decal
        /// - NOT exact geometry test
        /// </summary>
        public static void GetNearestTriangle(
            Vector3 p,
            Vector3[] vertices,
            int[] triangles,
            out Vector3 t1,
            out Vector3 t2,
            out Vector3 t3
        )
        {
            float minDist = float.MaxValue;
            t1 = t2 = t3 = Vector3.zero;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i + 1]];
                Vector3 c = vertices[triangles[i + 2]];

                Vector3 center = (a + b + c) / 3f;
                float d = (p - center).sqrMagnitude;

                if (d < minDist)
                {
                    minDist = d;
                    t1 = a;
                    t2 = b;
                    t3 = c;
                }
            }
        }

        // =====================================================
        // CLAMP / PROJECTION
        // =====================================================

        /// <summary>
        /// Clamp a point into triangle area.
        /// 
        /// Used when:
        /// - Brush center is slightly outside
        /// - Want stable paint result
        ///
        /// NOT exact mathematical projection.
        /// </summary>
        public static Vector3 ClampPointToTriangle(
            Vector3 p,
            Vector3 t1,
            Vector3 t2,
            Vector3 t3
        )
        {
            Vector3 center = (t1 + t2 + t3) / 3f;
            return Vector3.Lerp(center, p, 0.85f);
        }
        
        // Example usage:
        //     RaycastHit hit;
        //     if (Physics.Raycast(ray, out hit))
        // {
        //     Mesh mesh = hit.collider.GetComponent<MeshFilter>().sharedMesh;
        //
        //     int triIndex = hit.triangleIndex * 3;
        //
        //     Vector3 t1 = mesh.vertices[mesh.triangles[triIndex]];
        //     Vector3 t2 = mesh.vertices[mesh.triangles[triIndex + 1]];
        //     Vector3 t3 = mesh.vertices[mesh.triangles[triIndex + 2]];
        //
        //     Vector2 uv = MeshPaintMath.CalculateUV(
        //         hit.point,
        //         t1, mesh.uv[mesh.triangles[triIndex]],
        //         t2, mesh.uv[mesh.triangles[triIndex + 1]],
        //         t3, mesh.uv[mesh.triangles[triIndex + 2]]
        //     );
        //
        //     // 👉 uv dùng để paint texture / decal
        // }
    }
}
