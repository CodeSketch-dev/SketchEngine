using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SketchEngine.Utilities
{
    /// <summary>Chuyển tọa độ world, screen và UI. Không tạo đối tượng cập nhật mỗi frame.</summary>
    public static class UtilityCanvas
    {
        static RectTransform _canvas;
        static Camera _camera;
        static Canvas _canvasComp;
        static bool _explicitCanvas, _explicitCamera;
        static int _canvasSearchFrame = -1, _cameraSearchFrame = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            _canvas = null;
            _canvasComp = null;
            _camera = null;
            _explicitCanvas = _explicitCamera = false;
            _canvasSearchFrame = _cameraSearchFrame = -1;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        static void OnActiveSceneChanged(Scene previous, Scene current) => InvalidateAutomaticCache();
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InvalidateAutomaticCache();
        static void OnSceneUnloaded(Scene scene) => InvalidateAutomaticCache();

        static void InvalidateAutomaticCache()
        {
            if (!_explicitCanvas) { _canvas = null; _canvasComp = null; }
            if (!_explicitCamera) _camera = null;
            _canvasSearchFrame = _cameraSearchFrame = -1;
        }

        /// <summary>
        /// Gán Canvas và camera world mặc định để tránh tìm kiếm tự động và chọn nhầm khi có nhiều Canvas.
        /// Truyền null để quay lại chế độ tìm tự động. Camera UI lấy từ Canvas.worldCamera.
        /// </summary>
        public static void SetContext(Canvas canvas, Camera worldCamera = null)
        {
            _canvasComp = canvas;
            _canvas = canvas != null ? canvas.transform as RectTransform : null;
            _camera = worldCamera;
            _explicitCanvas = canvas != null;
            _explicitCamera = worldCamera != null;
            _canvasSearchFrame = _cameraSearchFrame = -1;
        }

        /// <summary>Xóa kết quả tìm tự động, dùng sau khi thêm hoặc đổi Canvas/camera lúc runtime.</summary>
        public static void RefreshCache() => InvalidateAutomaticCache();

        static Camera ResolveCamera(Camera camera = null)
        {
            if (camera != null) return camera;
            if (_camera != null && _camera.isActiveAndEnabled) return _camera;
            if (_explicitCamera && _camera != null) return null;
            _explicitCamera = false;
            if (_cameraSearchFrame == Time.frameCount) return null;
            _cameraSearchFrame = Time.frameCount;
            _camera = Camera.main;
            if (_camera == null)
                _camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            return _camera != null && _camera.isActiveAndEnabled ? _camera : null;
        }

        static RectTransform ResolveCanvas(RectTransform rect = null)
        {
            if (rect != null) return rect;
            if (_canvas != null && _canvasComp != null && _canvasComp.isActiveAndEnabled) return _canvas;
            if (_explicitCanvas && _canvasComp != null) return null;
            _explicitCanvas = false;
            _canvas = null;
            _canvasComp = null;
            if (_canvasSearchFrame == Time.frameCount) return null;
            _canvasSearchFrame = Time.frameCount;
            // Chỉ tìm khi cache không hợp lệ; không sắp xếp kết quả theo InstanceID.
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var activeScene = SceneManager.GetActiveScene();
            int bestScore = -1;
            foreach (var candidate in canvases)
            {
                if (!candidate.isActiveAndEnabled || !candidate.isRootCanvas) continue;
                int score = (candidate.gameObject.scene == activeScene ? 2 : 0) +
                    (candidate.renderMode == RenderMode.ScreenSpaceOverlay ? 1 : 0);
                if (score <= bestScore) continue;
                bestScore = score;
                _canvasComp = candidate;
            }
            if (_canvasComp != null) _canvas = _canvasComp.transform as RectTransform;
            return _canvas;
        }

        public static Camera MainCamera => ResolveCamera();
        public static RectTransform Canvas => ResolveCanvas();
        public static Canvas CanvasComponent { get { ResolveCanvas(); return _canvasComp; } }

        static bool TryGetUICamera(RectTransform rect, Camera overrideCamera, out Camera camera)
        {
            camera = null;
            if (rect == null) return false;
            var owner = rect == _canvas ? _canvasComp : rect.GetComponentInParent<Canvas>();
            if (owner == null) return false;
            var root = owner.rootCanvas;
            // Overlay sử dụng tọa độ màn hình trực tiếp, tuyệt đối không truyền camera world.
            if (root.renderMode == RenderMode.ScreenSpaceOverlay) return true;
            camera = overrideCamera != null ? overrideCamera : root.worldCamera;
            if (camera == null) camera = ResolveCamera();
            return camera != null;
        }

        static InvalidOperationException MissingContext() => new InvalidOperationException(
            "UtilCanvas: thiếu camera, Canvas hoặc mặt phẳng UI hợp lệ. Gán SetContext hoặc dùng hàm Try để xử lý thất bại.");

        /// <summary>Chuyển world sang screen (pixel). Camera truyền vào là camera world.</summary>
        public static Vector2 WorldToScreen(Vector3 worldPos, Camera cam = null)
        {
            cam = ResolveCamera(cam);
            if (cam == null) throw MissingContext();
            return cam.WorldToScreenPoint(worldPos);
        }

        /// <summary>Chuyển screen sang tọa độ local của rect; không phải anchoredPosition của mọi target.</summary>
        public static bool TryScreenToLocal(Vector2 screenPos, out Vector2 localPoint,
            RectTransform canvas = null, Camera uiCamera = null)
        {
            localPoint = default;
            canvas = ResolveCanvas(canvas);
            return TryGetUICamera(canvas, uiCamera, out var camera) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screenPos, camera, out localPoint);
        }

        /// <summary>
        /// API tương thích: trả tọa độ local của Canvas/rect. Chỉ dùng trực tiếp cho anchoredPosition
        /// nếu điểm tham chiếu anchor của target trùng gốc local. Trường hợp khác dùng TryScreenToAnchoredPosition.
        /// cam là camera UI; Overlay luôn bỏ qua camera.
        /// </summary>
        public static Vector2 ScreenToAnchored(Vector2 screenPos, RectTransform canvas = null, Camera cam = null)
        {
            if (!TryScreenToLocal(screenPos, out var local, canvas, cam)) throw MissingContext();
            return local;
        }

        /// <summary>API tương thích: world sang tọa độ local của Canvas/rect; cam là camera world.</summary>
        public static Vector2 WorldToAnchored(Vector3 worldPos, RectTransform canvas = null, Camera cam = null)
        {
            var screen = WorldToScreen(worldPos, cam);
            return ScreenToAnchored(screen, canvas);
        }

        static Vector2 AnchorReference(RectTransform target, RectTransform parent)
        {
            var anchor = new Vector2(
                Mathf.LerpUnclamped(target.anchorMin.x, target.anchorMax.x, target.pivot.x),
                Mathf.LerpUnclamped(target.anchorMin.y, target.anchorMax.y, target.pivot.y));
            return parent.rect.min + Vector2.Scale(parent.rect.size, anchor);
        }

        /// <summary>
        /// Trả anchoredPosition đúng theo parent, anchor và pivot của target, kể cả anchor kéo giãn.
        /// Đặt target.anchoredPosition bằng kết quả; target nên nằm trên mặt phẳng local Z = 0 của parent.
        /// </summary>
        public static bool TryScreenToAnchoredPosition(Vector2 screenPos, RectTransform target,
            out Vector2 anchoredPosition, Camera uiCamera = null)
        {
            anchoredPosition = default;
            if (target == null || !(target.parent is RectTransform parent)) return false;
            if (!TryScreenToLocal(screenPos, out var local, parent, uiCamera)) return false;
            anchoredPosition = local - AnchorReference(target, parent);
            return true;
        }

        /// <summary>Đặt UI theo điểm world; dùng riêng camera world và camera UI của Canvas đích.</summary>
        public static bool TryWorldToAnchoredPosition(Vector3 worldPos, RectTransform target,
            out Vector2 anchoredPosition, Camera worldCamera = null, Camera uiCamera = null)
        {
            anchoredPosition = default;
            var camera = ResolveCamera(worldCamera);
            if (camera == null) return false;
            var screen = camera.WorldToScreenPoint(worldPos);
            if (screen.z <= 0f) return false;
            return TryScreenToAnchoredPosition(screen, target, out anchoredPosition, uiCamera);
        }

        /// <summary>Chuyển vị trí pivot của rect nguồn sang tọa độ local của Canvas/rect đích.</summary>
        public static Vector2 RectTransformToAnchored(RectTransform rect, RectTransform canvas = null, Camera cam = null)
        {
            if (!TryGetUICamera(rect, cam, out var sourceCamera)) throw MissingContext();
            var screen = RectTransformUtility.WorldToScreenPoint(sourceCamera, rect.position);
            return ScreenToAnchored(screen, canvas);
        }

        /// <summary>Tạo ray từ camera world xuyên qua vị trí màn hình của pivot UI.</summary>
        public static Ray RectTransformToRay(RectTransform rect, Camera cam = null)
        {
            var worldCamera = ResolveCamera(cam);
            if (worldCamera == null || !TryGetUICamera(rect, null, out var uiCamera)) throw MissingContext();
            var screen = RectTransformUtility.WorldToScreenPoint(uiCamera, rect.position);
            return worldCamera.ScreenPointToRay(screen);
        }

        /// <summary>
        /// Giới hạn anchoredPosition để bốn góc target nằm trong rect Canvas, có xét pivot, scale và rotation.
        /// Không sửa Transform. Nếu target lớn hơn Canvas, căn giữa theo trục không thể chứa vừa.
        /// </summary>
        public static Vector2 ClampAnchoredToCanvas(Vector2 anchored, RectTransform target, RectTransform canvas = null)
        {
            canvas = ResolveCanvas(canvas);
            if (canvas == null || target == null || !(target.parent is RectTransform parent)) throw MissingContext();
            var matrix = canvas.worldToLocalMatrix * target.localToWorldMatrix;
            var rect = target.rect;
            var a = matrix.MultiplyPoint3x4(new Vector3(rect.xMin, rect.yMin));
            var b = matrix.MultiplyPoint3x4(new Vector3(rect.xMin, rect.yMax));
            var c = matrix.MultiplyPoint3x4(new Vector3(rect.xMax, rect.yMax));
            var d = matrix.MultiplyPoint3x4(new Vector3(rect.xMax, rect.yMin));
            var parentToCanvas = canvas.worldToLocalMatrix * parent.localToWorldMatrix;
            var delta = parentToCanvas.MultiplyVector((Vector3)(anchored - target.anchoredPosition));
            var min = Vector2.Min(Vector2.Min(a, b), Vector2.Min(c, d)) + (Vector2)delta;
            var max = Vector2.Max(Vector2.Max(a, b), Vector2.Max(c, d)) + (Vector2)delta;
            var limits = canvas.rect;
            var correction = new Vector2(
                ClampCorrection(min.x, max.x, limits.xMin, limits.xMax),
                ClampCorrection(min.y, max.y, limits.yMin, limits.yMax));
            // Giải phép chiếu XY của parent sang Canvas: không trộn scale world với đơn vị UI.
            float determinant = parentToCanvas.m00 * parentToCanvas.m11 - parentToCanvas.m01 * parentToCanvas.m10;
            if (Mathf.Abs(determinant) < 0.000001f) return anchored;
            return anchored + new Vector2(
                (parentToCanvas.m11 * correction.x - parentToCanvas.m01 * correction.y) / determinant,
                (parentToCanvas.m00 * correction.y - parentToCanvas.m10 * correction.x) / determinant);
        }

        static float ClampCorrection(float min, float max, float lower, float upper)
        {
            if (max - min > upper - lower) return (lower + upper - min - max) * 0.5f;
            if (min < lower) return lower - min;
            if (max > upper) return upper - max;
            return 0f;
        }

        /// <summary>Kiểm tra điểm nằm trong vùng pixel của camera và khoảng near/far clip; margin tính bằng pixel.</summary>
        public static bool IsWorldOnScreen(Vector3 worldPos, Camera cam = null, float margin = 0f)
        {
            cam = ResolveCamera(cam);
            if (cam == null) return false;
            var screen = cam.WorldToScreenPoint(worldPos);
            if (screen.z < cam.nearClipPlane || screen.z > cam.farClipPlane) return false;
            var rect = cam.pixelRect;
            return screen.x >= rect.xMin - margin && screen.x <= rect.xMax + margin &&
                screen.y >= rect.yMin - margin && screen.y <= rect.yMax + margin;
        }
    }
}