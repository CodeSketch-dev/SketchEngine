using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SketchEngine.Editor
{
    /// <summary>
    /// Bộ công cụ tiện ích cho Editor trong GOCD Framework
    /// </summary>
    public static class Editor_MenuUtilities
    {
        #region Data

        [MenuItem("SketchEngine/Data/Clear PlayerPrefs", false)]
        static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("✅ Đã xoá toàn bộ PlayerPrefs");
        }

        [MenuItem("SketchEngine/Data/Clear Game Data", false)]
        static void ClearGameData()
        {
            DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);

            foreach (FileInfo file in di.GetFiles())
                file.Delete();

            foreach (DirectoryInfo dir in di.GetDirectories())
                dir.Delete(true);

            Debug.Log("✅ Đã xoá toàn bộ file trong persistentDataPath");
        }

        [MenuItem("SketchEngine/Data/Clear Caching", false)]
        static void ClearCache()
        {
            Caching.ClearCache();
            Debug.Log("✅ Đã xoá cache Unity (AssetBundle, Addressables...)");
        }

        [MenuItem("SketchEngine/Data/Clear All", false)]
        static void ClearAll()
        {
            ClearPlayerPrefs();
            ClearGameData();
            ClearCache();
        }

        [MenuItem("SketchEngine/Data/Open GameData Directory", false)]
        static void OpenGameData()
        {
            // Mở thư mục chứa dữ liệu game
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        #endregion

        #region Game Control

        static readonly float s_slowTimeScale = 0.1f;
        static bool s_slowed;

        [MenuItem("SketchEngine/Game/Pause or Resume _F2", false)]
        static void Pause()
        {
            if (!Application.isPlaying) return;

            if (!EditorApplication.isPaused)
                Debug.Break(); // Dừng frame hiện tại
            else
                EditorApplication.isPaused = false; // Tiếp tục
        }

        [MenuItem("SketchEngine/Game/Slow or Resume _F3", false)]
        static void Slow()
        {
            if (!Application.isPlaying) return;

            s_slowed = !s_slowed;
            Time.timeScale = s_slowed ? s_slowTimeScale : 1f;

            Debug.Log($"⚙ Time.timeScale = {Time.timeScale}");
        }

        [MenuItem("SketchEngine/Game/Reload Scene _F5", false)]
        static void ReloadScene()
        {
            if (!Application.isPlaying) return;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Debug.Log("🔄 Scene reloaded.");
        }

        #endregion

        #region Screenshot

        [MenuItem("SketchEngine/Capture Screenshot _F4", false)]
        static void CaptureScreenshot()
        {
            // Tạo tên file dạng yyyy-MM-dd_HH-mm-ss.png
            string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture) + ".png";
            string path = Path.Combine("Assets", fileName);

            // Ghi ảnh màn hình vào file PNG
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"📸 Screenshot đang lưu: {path}");

            // Chờ vài frame để chắc chắn ảnh được ghi, rồi mới refresh Project view
            EditorApplication.delayCall += TryRefreshScreenshot;

            void TryRefreshScreenshot()
            {
                if (File.Exists(path))
                {
                    AssetDatabase.Refresh();
                    Debug.Log("✅ Screenshot đã được load vào Project.");
                }
                else
                {
                    // Nếu ảnh chưa được ghi xong, đợi frame sau rồi thử lại
                    EditorApplication.delayCall += TryRefreshScreenshot;
                }
            }
        }

        #endregion
    }
}
