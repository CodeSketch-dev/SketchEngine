using UnityEngine;

namespace SketchEngine.Utilities
{
    /// <summary>
    /// Gắn component này vào bất kỳ object nào để set target frame rate và vsync count.
    /// Lưu ý: vsync count phải bằng 0 thì target FPS mới có tác dụng.
    ///
    /// Tự áp lại setting sau khi app resume (OnApplicationPause/OnApplicationFocus), vì nhiều SDK
    /// quảng cáo (AdMob, Unity Ads...) tự ý đổi Application.targetFrameRate khi đóng rewarded/
    /// interstitial - nếu không recheck, game sẽ bị tụt fps vĩnh viễn sau khi xem quảng cáo.
    /// </summary>
    public class FpsUnlock : MonoBehaviour
    {
        /// <summary>Target FPS mong muốn. -1 = không giới hạn (uncapped); phải >= 1 nếu muốn giới hạn cụ thể.</summary>
        [SerializeField] int _targetFPS = 60;
        [Range(0, 2)]
        [SerializeField] int _vSyncCount = 0;

        protected virtual void Start()
        {
            UpdateSettings();
        }

        protected virtual void OnValidate()
        {
            // Không set 0, vì Application.targetFrameRate = 0 khiến game gần như đứng hình.
            _targetFPS = _targetFPS < 0 ? -1 : Mathf.Max(_targetFPS, 1);

            // Lúc đang Play thì áp dụng ngay để tiện chỉnh trực tiếp; lúc edit-mode set thì vô nghĩa với runtime nên bỏ qua.
            if (Application.isPlaying)
                UpdateSettings();
        }

        /// <summary>Một số SDK quảng cáo tự đổi targetFrameRate khi đóng quảng cáo - áp lại khi app quay lại foreground.</summary>
        protected virtual void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) UpdateSettings();
        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus) UpdateSettings();
        }

        protected virtual void UpdateSettings()
        {
            QualitySettings.vSyncCount = _vSyncCount;
            Application.targetFrameRate = _targetFPS;
        }
    }
}
