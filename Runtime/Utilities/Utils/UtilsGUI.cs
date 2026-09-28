using SketchEngine.Mono;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SketchEngine.Utilities.Utils
{
    public static class UtilsGUI
    {
        static RectTransform _canvas;
        static Camera _camera;
        static Canvas _canvasComp;

        // =====================================================
        // INIT
        // =====================================================

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Initialize()
        {
            MonoCallback.SafeInstance.EventActiveSceneChanged += MonoCallback_ActiveSceneChanged;
        }

        static void MonoCallback_ActiveSceneChanged(Scene prev, Scene now)
        {
            CacheCanvas();
            CacheCamera();
        }
        
        static void CacheCanvas()
        {
            var canvases = Object.FindObjectsOfType<Canvas>();
            if (canvases == null || canvases.Length == 0)
                return;

            // Ưu tiên Overlay
            foreach (var c in canvases)
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    _canvasComp = c;
                    _canvas = c.transform as RectTransform;
                    return;
                }
            }

            // Fallback
            _canvasComp = canvases[0];
            _canvas = canvases[0].transform as RectTransform;
        }

        static void CacheCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
                _camera = Object.FindObjectOfType<Camera>();
        }

        // =====================================================
        // BASIC
        // =====================================================

        public static Camera MainCamera => _camera;
        public static RectTransform Canvas => _canvas;
        public static Canvas CanvasComponent => _canvasComp;

        // =====================================================
        // WORLD → SCREEN / UI
        // =====================================================

        /// <summary>
        /// World → Screen
        /// </summary>
        public static Vector2 WorldToScreen(Vector3 worldPos, Camera cam = null)
        {
            return (cam ?? _camera).WorldToScreenPoint(worldPos);
        }

        /// <summary>
        /// World → AnchoredPosition (chuẩn UI)
        /// </summary>
        public static Vector2 WorldToAnchored(
            Vector3 worldPos,
            RectTransform canvas = null,
            Camera cam = null)
        {
            canvas ??= _canvas;
            cam ??= _camera;

            Vector2 screen = cam.WorldToScreenPoint(worldPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas,
                screen,
                _canvasComp.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
                out var localPoint
            );

            return localPoint;
        }

        // =====================================================
        // SCREEN → UI
        // =====================================================

        public static Vector2 ScreenToAnchored(
            Vector2 screenPos,
            RectTransform canvas = null,
            Camera cam = null)
        {
            canvas ??= _canvas;
            cam ??= _camera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas,
                screenPos,
                _canvasComp.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
                out var localPoint
            );

            return localPoint;
        }

        // =====================================================
        // RECTTRANSFORM
        // =====================================================

        public static Vector2 RectTransformToAnchored(
            RectTransform rect,
            RectTransform canvas = null,
            Camera cam = null)
        {
            return ScreenToAnchored(
                RectTransformUtility.WorldToScreenPoint(cam ?? _camera, rect.position),
                canvas,
                cam
            );
        }

        public static Ray RectTransformToRay(
            RectTransform rect,
            Camera cam = null)
        {
            cam ??= _camera;
            return cam.ScreenPointToRay(
                RectTransformUtility.WorldToScreenPoint(cam, rect.position)
            );
        }

        // =====================================================
        // UTILS (RẤT HAY DÙNG)
        // =====================================================

        /// <summary>
        /// Clamp anchoredPosition trong canvas
        /// </summary>
        public static Vector2 ClampAnchoredToCanvas(
            Vector2 anchored,
            RectTransform target,
            RectTransform canvas = null)
        {
            canvas ??= _canvas;

            var canvasRect = canvas.rect;
            var size = target.rect.size * target.lossyScale;

            float halfW = size.x * 0.5f;
            float halfH = size.y * 0.5f;

            anchored.x = Mathf.Clamp(
                anchored.x,
                canvasRect.xMin + halfW,
                canvasRect.xMax - halfW
            );

            anchored.y = Mathf.Clamp(
                anchored.y,
                canvasRect.yMin + halfH,
                canvasRect.yMax - halfH
            );

            return anchored;
        }

        /// <summary>
        /// Check world position có nằm trong screen không
        /// </summary>
        public static bool IsWorldOnScreen(
            Vector3 worldPos,
            Camera cam = null,
            float margin = 0f)
        {
            cam ??= _camera;
            var sp = cam.WorldToScreenPoint(worldPos);

            if (sp.z < 0f) return false;

            return sp.x >= -margin &&
                   sp.x <= Screen.width + margin &&
                   sp.y >= -margin &&
                   sp.y <= Screen.height + margin;
        }
    }
}
