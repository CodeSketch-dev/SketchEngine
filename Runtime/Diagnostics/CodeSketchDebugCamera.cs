using SketchEngine.Core.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SketchEngine.Diagnostics
{
    [ExecuteAlways]
    public class CodeSketchDebugCamera : MonoBehaviour
    {
        [Tooltip("Khoảng cách từ camera để vẽ khung hiển thị.")]
        public float _distance = 10f;

        [Tooltip("Màu khung hiển thị.")]
        public Color gizmoColor = new Color(1f, 0.5f, 0f, 0.4f);

        [Tooltip("Luôn vẽ khung hay chỉ khi được chọn?")]
        public bool alwaysDraw = true;

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!alwaysDraw) return;
            DrawViewFrustum();
        }

        void OnDrawGizmosSelected()
        {
            if (alwaysDraw) return;
            DrawViewFrustum();
        }

        void DrawViewFrustum()
        {
            if (Camera.current == null || !(GetComponent<Camera>() is Camera cam) || !cam.orthographic == false)
                return;

            Vector3 pos = cam.transform.position;
            Vector3 fwd = cam.transform.forward;
            Vector2 size = cam.SizeAtDistance(_distance);

            Vector3 center = pos + fwd * _distance;
            Vector3 right = cam.transform.right * size.x * 0.5f;
            Vector3 up = cam.transform.up * size.y * 0.5f;

            // 4 corners
            Vector3 tl = center + up - right;
            Vector3 tr = center + up + right;
            Vector3 bl = center - up - right;
            Vector3 br = center - up + right;

            Handles.color = gizmoColor;
            Handles.DrawSolidRectangleWithOutline(new[] { tl, tr, br, bl }, gizmoColor * 0.4f, gizmoColor);

            // Draw frustum lines
            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(pos, tl);
            Gizmos.DrawLine(pos, tr);
            Gizmos.DrawLine(pos, bl);
            Gizmos.DrawLine(pos, br);
        }
#endif
    }
}
