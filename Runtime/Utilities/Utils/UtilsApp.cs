using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SketchEngine.Utilities.Utils
{
    public static class UtilsApp
    {
#if UNITY_EDITOR
        /// <summary>
        /// Tìm tất cả assets thuộc kiểu T trong thư mục chỉ định (bao gồm thư mục con).
        /// </summary>
        public static List<T> GetAll<T>(string folderPath, params object[] args) where T : Object
        {
            List<T> objects = new List<T>();
            string hint = "";
            foreach (var s in args)
                hint += s.ToString() + " ";

            string searchData = $"t:{hint}";
            string[] guids = AssetDatabase.FindAssets(searchData, new[] { folderPath });

            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                T obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                objects.Add(obj);
            }

            // Đệ quy tìm trong thư mục con
            foreach (var subDirectory in AssetDatabase.GetSubFolders(folderPath))
                objects.AddRange(GetAll<T>(subDirectory, args));

            return objects;
        }
#endif

        /// <summary>Trả về hướng nhìn từ Quaternion.</summary>
        public static Vector3 RotationDirection(this Quaternion quaternion)
        {
            return quaternion * Vector3.forward;
        }

        /// <summary>Trả về hướng nhìn trên mặt phẳng XZ (bỏ Y).</summary>
        public static Vector3 DirectionXY(this Quaternion quaternion)
        {
            Vector3 direction = quaternion * Vector3.forward;
            direction.y = 0;
            return direction;
        }

        /// <summary>Chuyển vector hướng sang góc Y (Euler) theo trục XZ.</summary>
        public static float DirectionToEulersYByXZ(Vector3 direction)
        {
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            return angle < 0 ? angle + 360f : angle;
        }

        /// <summary>Chuyển vector hướng sang góc Y (Euler) theo trục ZX.</summary>
        public static float DirectionToEulersYByZX(Vector3 direction)
        {
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            return angle < 0 ? angle + 360f : angle;
        }

        /// <summary>Trộn ngẫu nhiên thứ tự mảng.</summary>
        public static void Shuffle<T>(T[] array)
        {
            System.Random rnd = new System.Random();
            int n = array.Length;
            while (n > 1)
            {
                int k = rnd.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
        }

        /// <summary>Xoá toàn bộ con của GameObject.</summary>
        public static void RemoveAllChild(GameObject gameObject)
        {
            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
                GameObject.Destroy(gameObject.transform.GetChild(i).gameObject);
        }

        /// <summary>Lấy version giữa trong Application.version (ví dụ 0.3.2 => 3).</summary>
        public static int GetAppVersion()
        {
            return int.Parse(Application.version.Split('.')[1]);
        }

        /// <summary>Định dạng số thực với 2 chữ số sau dấu phẩy.</summary>
        public static string FormatNumber(double number)
        {
            return string.Format("{0:0.00}", number);
        }

        /// <summary>Định dạng số nguyên.</summary>
        public static string FormatNumber(long number)
        {
            return string.Format("{0:0}", number);
        }

        /// <summary>Hiển thị dấu +/- phía trước số.</summary>
        public static string FormatNumberSign(long number)
        {
            string res = FormatNumber(Math.Abs(number));
            return number < 0 ? "-" + res : "+" + res;
        }

        /// <summary>Định dạng rút gọn đơn vị K, M, B (dùng cho số lớn).</summary>
        public static string FormatSortNumber(long number)
        {
            if (number >= 100_000 && number < 100_000_000)
                return FormatNumber(number / 1_000) + "K";
            else if (number >= 100_000_000 && number < 100_000_000_000)
                return FormatNumber(number / 1_000_000) + "M";
            else if (number >= 100_000_000_000)
                return FormatNumber(number / 1_000_000_000) + "B";
            return FormatNumber(number);
        }

        /// <summary>Phiên bản rút gọn dùng cho số nhỏ hơn.</summary>
        public static string FormatSortNumber2(long number)
        {
            if (number >= 10_000 && number < 10_000_000)
                return FormatNumber(number / 1_000) + "K";
            else if (number >= 10_000_000 && number < 10_000_000_000)
                return FormatNumber(number / 1_000_000) + "M";
            else if (number >= 10_000_000_000)
                return FormatNumber(number / 1_000_000_000) + "B";
            return FormatNumber(number);
        }
        
        /// <summary>Thời gian hiện tại (Unix time millis).</summary>
        public static long CurrentTimeMiliseconds()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        /// <summary>Thời gian hiện tại (Unix time seconds).</summary>
        public static int CurrentTimeSeconds()
        {
            return (int)(CurrentTimeMiliseconds() / 1000);
        }

        /// <summary>Trả về ngày hiện tại dạng yyyymmdd (UTC).</summary>
        public static int CurrentDateInt()
        {
            var now = DateTime.UtcNow;
            return now.Year * 10000 + now.Month * 100 + now.Day;
        }

        /// <summary>Trả về giờ hiện tại dạng hhmm (UTC).</summary>
        public static int CurrentTimeInt()
        {
            var now = DateTime.UtcNow;
            return now.Hour * 100 + now.Minute;
        }

        /// <summary>Kiểm tra có Internet không.</summary>
        public static bool InternetAvaiable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        /// <summary>Chuyển vị trí thế giới sang vị trí UI trong Canvas (Overlay).</summary>
        public static Vector2 WorldToUISpace(Canvas canvas, Vector3 worldPos)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, screenPos, canvas.worldCamera, out var localPos);
            return canvas.transform.TransformPoint(localPos);
        }

        /// <summary>Chuyển từ vị trí thế giới sang vị trí Canvas (Viewport space).</summary>
        public static Vector3 WorldToCanvasPosition(Canvas canvas, Vector3 worldPosition)
        {
            var viewportPosition = Camera.main.WorldToViewportPoint(worldPosition);
            return ViewportToCanvasPosition(canvas, viewportPosition);
        }

        /// <summary>Chuyển từ vị trí screen sang vị trí Canvas.</summary>
        public static Vector3 ScreenToCanvasPosition(Canvas canvas, Vector3 screenPosition)
        {
            var viewportPosition = new Vector3(screenPosition.x / Screen.width, screenPosition.y / Screen.height, 0);
            return ViewportToCanvasPosition(canvas, viewportPosition);
        }

        /// <summary>Chuyển từ Viewport (0-1) sang vị trí trên Canvas.</summary>
        public static Vector3 ViewportToCanvasPosition(Canvas canvas, Vector3 viewportPosition)
        {
            var centerOffset = viewportPosition - new Vector3(0.5f, 0.5f, 0);
            var scale = canvas.GetComponent<RectTransform>().sizeDelta;
            return Vector3.Scale(centerOffset, scale);
        }

        // --------------------- TỰ ĐỘNG BỔ SUNG -----------------------

        /// <summary>Clamp giá trị int trong khoảng 0..1.</summary>
        public static int Clamp01Int(int value) => Mathf.Clamp(value, 0, 1);

        /// <summary>Clamp giá trị float trong khoảng 0..1.</summary>
        public static float Clamp01(float value) => Mathf.Clamp01(value);

        /// <summary>Trả về thời gian hiện tại (UTC) dạng DateTime.</summary>
        public static DateTime NowUTC => DateTime.UtcNow;
    }
}
