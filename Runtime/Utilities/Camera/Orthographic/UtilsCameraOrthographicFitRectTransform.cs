using SketchEngine.Core.Extensions;
using SketchEngine.Mono;
using DG.Tweening;
using UnityEngine;

namespace SketchEngine.Utilities.CameraSystem
{
    /// <summary>
    /// Dùng để fit camera ortho sao cho khung nhìn vừa với RectTransform đích (dựa vào canvas).
    /// </summary>
    public class UtilsCameraOrthographicFitRectTransform : MonoBase
    {
        Sequence _sequence;

        void OnDestroy()
        {
            _sequence?.Kill();
        }

        /// <summary>
        /// Fit ngay lập tức (không animation) camera sao cho bounds vừa khít với RectTransform.
        /// </summary>
        /// <param name="cam">Camera orthographic</param>
        /// <param name="canvasRect">RectTransform gốc (canvas)</param>
        /// <param name="targetRect">RectTransform cần fit</param>
        /// <param name="bounds">Giới hạn vùng cần nhìn</param>
        /// <param name="fitPivot">Pivot chuẩn hóa (0-1) theo anchor mong muốn trong bounds</param>
        public void Fit(Camera cam, RectTransform canvasRect, RectTransform targetRect, Bounds bounds, Vector2 fitPivot)
        {
            Vector3[] canvasCorners = new Vector3[4];
            Vector3[] targetCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);
            targetRect.GetWorldCorners(targetCorners);

            Vector3 canvasSize = canvasCorners[2] - canvasCorners[0];
            Vector3 targetSize = targetCorners[2] - targetCorners[0];

            Vector3 canvasCenter = (canvasCorners[0] + canvasCorners[2]) * 0.5f;
            Vector3 targetCenter = (targetCorners[0] + targetCorners[2]) * 0.5f;

            float desiredWidth = (canvasSize.x / targetSize.x) * bounds.size.x;
            float desiredHeight = (canvasSize.y / targetSize.y) * bounds.size.y;

            cam.SetWidth(desiredWidth);

            if (cam.GetHeight() < desiredHeight)
                cam.SetHeight(desiredHeight);

            Vector3 desiredPosition = bounds.center;
            desiredPosition.x -= ((targetCenter.x - canvasCenter.x) / canvasSize.x) * cam.GetWidth();
            desiredPosition.y -= ((targetCenter.y - canvasCenter.y) / canvasSize.y) * cam.GetHeight();

            desiredPosition.x += bounds.size.x * (fitPivot.x - 0.5f) - targetSize.y * (fitPivot.x - 0.5f) / canvasSize.y * cam.GetWidth();
            desiredPosition.y += bounds.size.y * (fitPivot.y - 0.5f) - targetSize.y * (fitPivot.y - 0.5f) / canvasSize.y * cam.GetHeight();

            desiredPosition.z = cam.transform.position.z;

            cam.transform.position = desiredPosition;
        }

        /// <summary>
        /// Fit camera có animation (dùng DOTween để move và scale ortho)
        /// </summary>
        /// <param name="cam">Camera orthographic</param>
        /// <param name="canvasRect">RectTransform gốc</param>
        /// <param name="targetRect">RectTransform mục tiêu</param>
        /// <param name="bounds">Bounds cần fit</param>
        /// <param name="fitPivot">Pivot tương đối cần canh (0-1)</param>
        /// <param name="duration">Thời gian tween</param>
        /// <param name="ease">Ease dùng cho tween</param>
        public void Fit(Camera cam, RectTransform canvasRect, RectTransform targetRect, Bounds bounds, Vector2 fitPivot, float duration, Ease ease)
        {
            float lastOrthoSize = cam.orthographicSize;
            Vector3 lastPosition = cam.transform.position;

            Fit(cam, canvasRect, targetRect, bounds, fitPivot);

            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            _sequence.Append(cam.transform.DOMove(cam.transform.position, duration)
                .SetEase(ease)
                .ChangeStartValue(lastPosition));

            _sequence.Join(cam.DOOrthoSize(cam.orthographicSize, duration)
                .SetEase(ease)
                .ChangeStartValue(lastOrthoSize));
        }
    }
}
