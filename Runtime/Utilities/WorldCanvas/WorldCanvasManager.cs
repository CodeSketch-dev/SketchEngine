using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SketchEngine.Utilities.CanvasWorld
{
    /// <summary>
    /// Đảm bảo luôn có DUY NHẤT một Canvas WorldSpace toàn cục, sống qua các lần load scene,
    /// tự re-validate mỗi khi load scene mới, và tự resize theo hướng xoay màn hình (orientation).
    ///
    /// KHÔNG tự tạo canvas nếu game chưa từng cần tới nó, tránh rác 1 GameObject DontDestroyOnLoad
    /// cho những game không dùng world canvas. Canvas chỉ được tạo khi:
    /// 1) code gọi <see cref="WorldCanvasUtility.Push"/> lần đầu, hoặc
    /// 2) scene có đặt sẵn component <c>WorldCanvasEnabler</c> (singleton đánh dấu game này cần dùng).
    /// </summary>
    public static class WorldCanvasManager
    {
        const float LandscapeWidth = 1920f, LandscapeHeight = 1080f;
        const float PortraitWidth = 1080f, PortraitHeight = 1920f;

        public static Transform Root { get; set; }

        static Canvas _canvas;
        static RectTransform _rect;
        static ScreenOrientation _lastOrientation;

        // =====================================================
        // INIT (CHẠY 1 LẦN CHO MỖI LẦN MỞ APP)
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

        /// <summary>
        /// Chỉ refresh nếu canvas ĐÃ tồn tại (không tự tạo mới). Đây là lý do game không dùng world
        /// canvas sẽ không bao giờ thấy GameObject "CanvasWorld" xuất hiện trong scene.
        /// </summary>
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (Root == null) return;
            UpdateCanvasCamera();
            UpdateCanvasSizeIfNeeded();
        }

        /// <summary>Tạo canvas nếu chưa có (lazy), rồi refresh camera/size. Gọi khi thực sự cần dùng world canvas.</summary>
        public static void ForceRefresh()
        {
            EnsureCanvas();
            UpdateCanvasCamera();
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

            // Tick orientation mỗi frame: đổi hướng máy giữa scene (không reload) vẫn phải resize kịp,
            // chứ không chỉ chờ tới lần OnSceneLoaded kế tiếp.
            go.AddComponent<OrientationWatcher>();

            Root = go.transform;

            UpdateCanvasSize();
        }

        /// <summary>
        /// GraphicRaycaster trên Canvas WorldSpace CẦN worldCamera được gán, nếu không mọi raycast UI
        /// (click, pointer enter...) sẽ luôn trượt một cách âm thầm. Chỉ refresh mỗi lần load scene
        /// (không phải mỗi frame) vì Camera.main thực hiện tìm kiếm theo tag, không rẻ.
        /// </summary>
        static void UpdateCanvasCamera()
        {
            if (_canvas == null) return;

            Camera main = Camera.main;
            if (main != null) _canvas.worldCamera = main;
        }

        // =====================================================
        // SIZE / ORIENTATION
        // =====================================================

        static void UpdateCanvasSizeIfNeeded()
        {
            if (_lastOrientation == Screen.orientation) return;

            _lastOrientation = Screen.orientation;
            UpdateCanvasSize();
        }

        static void UpdateCanvasSize()
        {
            if (_rect == null)
                return;

            switch (Screen.orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                case ScreenOrientation.LandscapeRight:
                    _rect.sizeDelta = new Vector2(LandscapeWidth, LandscapeHeight);
                    break;

                case ScreenOrientation.Portrait:
                case ScreenOrientation.PortraitUpsideDown:
                    _rect.sizeDelta = new Vector2(PortraitWidth, PortraitHeight);
                    break;

                default:
                    // Nền tảng không báo orientation chuẩn (vd desktop/AutoRotation=Unknown) -> suy theo tỉ lệ màn hình.
                    bool isLandscape = Screen.width > Screen.height;
                    _rect.sizeDelta = isLandscape
                        ? new Vector2(LandscapeWidth, LandscapeHeight)
                        : new Vector2(PortraitWidth, PortraitHeight);
                    break;
            }
        }

        // =====================================================
        // PUBLIC API
        // =====================================================

        /// <summary>Ép resize thủ công (tùy chọn). Gọi khi đổi resolution hoặc camera.</summary>
        public static void Refresh() => UpdateCanvasSize();

        // =====================================================
        // ORIENTATION WATCHER (zero-GC sau khi tạo)
        // =====================================================

        /// <summary>Component nội bộ, chỉ để tick kiểm tra đổi hướng màn hình mỗi frame; không làm gì khác.</summary>
        sealed class OrientationWatcher : MonoBehaviour
        {
            void Update() => UpdateCanvasSizeIfNeeded();
        }
    }
}
