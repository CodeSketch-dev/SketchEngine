using System;
using UnityEngine;

namespace SketchEngine.Utilities.Paint
{
    /// <summary>
    /// MeshOperator
    /// 
    /// Purpose:
    /// - Query mesh geometry for PAINT / DECAL / HIT effects
    /// - Convert local-space point to UV
    /// - Find nearest surface point on mesh
    ///
    /// NOT for physics or collision replacement.
    /// </summary>
    public sealed class MeshOperator
    {
        // =====================================================
        // CACHED MESH DATA (NO GC)
        // =====================================================

        readonly Mesh _mesh;
        readonly int[] _triangles;
        readonly Vector3[] _vertices;
        readonly Vector2[] _uvs;

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public MeshOperator(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            _mesh = mesh;
            _triangles = mesh.triangles;
            _vertices = mesh.vertices;
            _uvs = mesh.uv;
        }

        // =====================================================
        // LOCAL POINT → UV
        // =====================================================

        /// <summary>
        /// Convert a LOCAL-space point on mesh surface to UV coordinate.
        /// 
        /// Typical usage:
        /// - Raycast hit
        /// - hit.point transformed to local-space
        /// - Convert to UV to paint texture / decal
        ///
        /// Example:
        /// MeshOperator op = new MeshOperator(mesh);
        /// Vector2 uv;
        /// if (op.LocalPointToUV(localHitPoint, out uv))
        /// {
        ///     // use uv to paint
        /// }
        /// </summary>
        public bool LocalPointToUV(Vector3 localPoint, out Vector2 uv)
        {
            uv = default;

            // Loop through triangles
            for (int i = 0; i < _triangles.Length; i += 3)
            {
                int i0 = _triangles[i];
                int i1 = _triangles[i + 1];
                int i2 = _triangles[i + 2];

                Vector3 t1 = _vertices[i0];
                Vector3 t2 = _vertices[i1];
                Vector3 t3 = _vertices[i2];

                // Check plane
                if (!MeshPaintMath.IsPointOnPlane(localPoint, t1, t2, t3))
                    continue;

                // Check inside triangle or on edge
                if (!MeshPaintMath.IsPointInTriangle(localPoint, t1, t2, t3) &&
                    !MeshPaintMath.IsPointOnTriangleEdge(localPoint, t1, t2, t3))
                    continue;

                // Interpolate UV
                uv = MeshPaintMath.CalculateUV(
                    localPoint,
                    t1, _uvs[i0],
                    t2, _uvs[i1],
                    t3, _uvs[i2]
                );

                return true;
            }

            return false;
        }

        // =====================================================
        // NEAREST SURFACE POINT
        // =====================================================

        /// <summary>
        /// Find the nearest LOCAL-space point on mesh surface.
        /// 
        /// Used for:
        /// - Brush snapping
        /// - Hit stabilization
        /// - Clamp paint position
        ///
        /// NOTE:
        /// - Heuristic method (fast)
        /// - Suitable for paint / decal
        /// </summary>
        public Vector3 NearestLocalSurfacePoint(Vector3 localPoint)
        {
            float minDist = float.MaxValue;
            Vector3 best = localPoint;

            for (int i = 0; i < _triangles.Length; i += 3)
            {
                Vector3 t1 = _vertices[_triangles[i]];
                Vector3 t2 = _vertices[_triangles[i + 1]];
                Vector3 t3 = _vertices[_triangles[i + 2]];

                // Soft clamp point into triangle
                Vector3 projected =
                    MeshPaintMath.ClampPointToTriangle(localPoint, t1, t2, t3);

                float d = (localPoint - projected).sqrMagnitude;
                if (d < minDist)
                {
                    minDist = d;
                    best = projected;
                }
            }

            return best;
        }
        
        // Usage
        //     RaycastHit hit;
        //     if (Physics.Raycast(ray, out hit))
        // {
        //     var mf = hit.collider.GetComponent<MeshFilter>();
        //     if (!mf) return;
        //
        //     Mesh mesh = mf.sharedMesh;
        //
        //     // Convert hit point to LOCAL space
        //     Vector3 localPoint = mf.transform.InverseTransformPoint(hit.point);
        //
        //     MeshOperator op = new MeshOperator(mesh);
        //
        //     if (op.LocalPointToUV(localPoint, out Vector2 uv))
        //     {
        //         // 👉 uv dùng để:
        //         // - Paint texture
        //         // - Decal
        //         // - Bullet hole
        //         Debug.Log("UV = " + uv);
        //     }
        // }

    }
}
