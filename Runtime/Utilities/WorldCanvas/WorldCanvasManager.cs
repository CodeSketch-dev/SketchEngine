using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SketchEngine.Utilities.CanvasWorld
{
    /// <summary>
    /// WorldCanvasManager
    /// 
    /// Purpose:
    /// - Ensure ONE global WorldSpace Canvas
    /// - Survives scene loads
    /// - Re-validates on every scene load
    /// - Auto-resize by screen orientation
    /// </summary>
    public static class WorldCanvasManager
    {
        public static Transform Root { get; set; }

        static Canvas _canvas;
        static RectTransform _rect;
        static ScreenOrientation _lastOrientation;

        // =====================================================
        // INIT (ONCE PER APP)
        // =====================================================

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Bootstrap()
        {
            Root = null;
            _canvas = null;
            _rect = null;
            _lastOrientation = Screen.orientation;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // =====================================================
        // SCENE CALLBACK
        // =====================================================

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ForceRefresh();
        }

        public static void ForceRefresh()
        {
            EnsureCanvas();
            UpdateCanvasSizeIfNeeded();
        }

        // =====================================================
        // CORE
        // =====================================================

        static void EnsureCanvas()
        {
            if (Root != null)
                return;

            var go = new GameObject(
                "CanvasWorld",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

            Object.DontDestroyOnLoad(go);

            _canvas = go.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;

            _rect = go.GetComponent<RectTransform>();
            go.transform.localScale = Vector3.one * 0.01f;

            Root = go.transform;

            UpdateCanvasSize();
        }

        // =====================================================
        // SIZE / ORIENTATION
        // =====================================================

        static void UpdateCanvasSizeIfNeeded()
        {
            if (_lastOrientation != Screen.orientation)
            {
                _lastOrientation = Screen.orientation;
                UpdateCanvasSize();
            }
        }

        static void UpdateCanvasSize()
        {
            if (_rect == null)
                return;

            switch (Screen.orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                case ScreenOrientation.LandscapeRight:
                    _rect.sizeDelta = new Vector2(1280, 720);
                    break;

                case ScreenOrientation.Portrait:
                case ScreenOrientation.PortraitUpsideDown:
                    _rect.sizeDelta = new Vector2(720, 1280);
                    break;

                default:
                    // fallback theo aspect
                    float aspect = (float)Screen.width / Screen.height;
                    _rect.sizeDelta = aspect > 1f
                        ? new Vector2(1280, 720)
                        : new Vector2(720, 1280);
                    break;
            }
        }

        // =====================================================
        // PUBLIC API
        // =====================================================

        /// <summary>
        /// Force refresh size manually (optional).
        /// Call when changing resolution or camera.
        /// </summary>
        public static void Refresh()
        {
            UpdateCanvasSize();
        }
    }
}
