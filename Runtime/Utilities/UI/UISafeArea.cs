using Sirenix.OdinInspector;
using UnityEngine;
using SketchEngine.Diagnostics;

namespace SketchEngine.Utilities.UI
{
    /// <summary>
    /// Điều chỉnh anchor của panel theo vùng an toàn trên thiết bị có tai thỏ.
    /// Panel cha cần phủ toàn màn hình; giữ nguyên offset như cách hoạt động ban đầu.
    /// Để nội dung khớp vùng an toàn, đặt offset của panel bằng 0 trong Inspector.
    /// Đặt ảnh nền toàn màn hình bên ngoài panel này và các phần tử nội dung bên trong.
    /// Tắt Conform trên một trục để anchor trải hết màn hình theo trục đó.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class UISafeArea : MonoBehaviour
    {
        [Title("Config")]
        [SerializeField] bool _conformX = true; // Áp dụng vùng an toàn trên trục X.
        [SerializeField] bool _conformY = true; // Áp dụng vùng an toàn trên trục Y.

        [Space]
        [SerializeField] bool _logging = false; // Ghi log sau khi áp dụng vùng an toàn.

        /// <summary>
        /// Thiết bị mô phỏng trong Editor, tương ứng với Safe Area Helper của Crystal Pug.
        /// </summary>
        public enum SimDevice
        {
            None,
            iPhoneX,
            iPhoneXsMax,
            Pixel3XL_LSL,
            Pixel3XL_LSR
        }

        /// <summary>
        /// Chọn thiết bị mô phỏng cho tất cả UISafeArea; không ảnh hưởng bản build.
        /// Để None khi dùng Unity Device Simulator để lấy vùng an toàn của thiết bị đang chọn.
        /// </summary>
        public static SimDevice Sim = SimDevice.None;

#if UNITY_EDITOR
        // Mỗi bảng gồm vùng an toàn chuẩn hóa cho màn hình dọc và ngang.
        // Giữ nguyên dữ liệu và ánh xạ hướng từ bản Crystal Pug được import.
        static readonly Rect[] NSA_iPhoneX =
        {
            new Rect(0f, 102f / 2436f, 1f, 2202f / 2436f),
            new Rect(132f / 2436f, 63f / 1125f, 2172f / 2436f, 1062f / 1125f)
        };

        static readonly Rect[] NSA_iPhoneXsMax =
        {
            new Rect(0f, 102f / 2688f, 1f, 2454f / 2688f),
            new Rect(132f / 2688f, 63f / 1242f, 2424f / 2688f, 1179f / 1242f)
        };

        static readonly Rect[] NSA_Pixel3XL_LSL =
        {
            new Rect(0f, 0f, 1f, 2789f / 2960f),
            new Rect(0f, 0f, 2789f / 2960f, 1f)
        };

        static readonly Rect[] NSA_Pixel3XL_LSR =
        {
            new Rect(0f, 0f, 1f, 2789f / 2960f),
            new Rect(171f / 2960f, 0f, 2789f / 2960f, 1f)
        };
#endif

        RectTransform _rectTransform;
        Rect _lastSafeArea;
        Vector2Int _lastScreenSize;
        ScreenOrientation _lastOrientation;
        bool _lastConformX;
        bool _lastConformY;
        bool _hasApplied;

        void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();
            _hasApplied = false;
            Refresh();
        }

        void LateUpdate()
        {
            Refresh();
        }

        void Refresh()
        {
            Rect safeArea = GetSafeArea();
            int width = Screen.width;
            int height = Screen.height;
            ScreenOrientation orientation = Screen.orientation;

            if (_hasApplied && safeArea == _lastSafeArea
                && width == _lastScreenSize.x && height == _lastScreenSize.y
                && orientation == _lastOrientation
                && _conformX == _lastConformX && _conformY == _lastConformY)
                return;

            if (!ApplySafeArea(safeArea, width, height))
                return;

            // Chỉ lưu trạng thái sau khi áp dụng thành công để dữ liệu lỗi được thử lại.
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(width, height);
            _lastOrientation = orientation;
            _lastConformX = _conformX;
            _lastConformY = _conformY;
            _hasApplied = true;
        }

        Rect GetSafeArea()
        {
            Rect safeArea = Screen.safeArea;
#if UNITY_EDITOR
            if (Sim != SimDevice.None)
            {
                int index = Screen.height > Screen.width ? 0 : 1;
                Rect normalizedArea;
                switch (Sim)
                {
                    case SimDevice.iPhoneX:
                        normalizedArea = NSA_iPhoneX[index];
                        break;
                    case SimDevice.iPhoneXsMax:
                        normalizedArea = NSA_iPhoneXsMax[index];
                        break;
                    case SimDevice.Pixel3XL_LSL:
                        normalizedArea = NSA_Pixel3XL_LSL[index];
                        break;
                    case SimDevice.Pixel3XL_LSR:
                        normalizedArea = NSA_Pixel3XL_LSR[index];
                        break;
                    default:
                        return safeArea;
                }

                safeArea = new Rect(
                    Screen.width * normalizedArea.x, Screen.height * normalizedArea.y,
                    Screen.width * normalizedArea.width, Screen.height * normalizedArea.height);
            }
#endif
            return safeArea;
        }

        bool ApplySafeArea(Rect area, int width, int height)
        {
            if (_rectTransform == null || width <= 0 || height <= 0
                || !IsFinite(area.xMin) || !IsFinite(area.yMin)
                || !IsFinite(area.xMax) || !IsFinite(area.yMax)
                || area.width <= 0 || area.height <= 0)
                return false;

            // Giữ cách xử lý gốc: trục không áp dụng vùng an toàn trải hết màn hình.
            if (!_conformX)
            {
                area.x = 0;
                area.width = width;
            }

            if (!_conformY)
            {
                area.y = 0;
                area.height = height;
            }

            Vector2 anchorMin = new Vector2(area.xMin / width, area.yMin / height);
            Vector2 anchorMax = new Vector2(area.xMax / width, area.yMax / height);
            if (!IsFinite(anchorMin.x) || !IsFinite(anchorMin.y)
                || !IsFinite(anchorMax.x) || !IsFinite(anchorMax.y)
                || anchorMin.x < 0 || anchorMin.y < 0
                || anchorMax.x < anchorMin.x || anchorMax.y < anchorMin.y
                || anchorMax.x > 1 || anchorMax.y > 1)
                return false;

            // Chỉ đổi anchor, giữ nguyên offset và pivot của panel như script ban đầu.
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;

            if (_logging)
                SketchDebug.Log<UISafeArea>($"Đã áp dụng vùng an toàn cho {name}: {area}", Color.cyan);

            return true;
        }

        static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
