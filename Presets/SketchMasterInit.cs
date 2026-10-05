using SketchEngine.Audio;
using SketchEngine.Core;
using SketchEngine.Settings;
using SketchEngine.UIPopup;
using UnityEngine;

namespace SketchEngine.Preset
{
    public static class SketchMasterInit
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void RuntimeInit()
        {
            if (Application.isMobilePlatform)
            {
                Application.targetFrameRate = Mathf.Min(Screen.currentResolution.refreshRate, 60);
                QualitySettings.vSyncCount = 0;
            }

            InitSettings();
            InitSRDebug();
        }

        #region Vibration Setting

        static void InitSettings()
        {
            AudioManager.VolumnMusic.Value = DataSettings.MusicVolume.Value;
            AudioManager.VolumeSound.Value = DataSettings.SoundVolume.Value;

            DataSettings.MusicVolume.OnValueChanged += (volume) => { AudioManager.VolumnMusic.Value = volume; };
            DataSettings.SoundVolume.OnValueChanged += (volume) => { AudioManager.VolumeSound.Value = volume; };

            Taptic.tapticOn = DataSettings.Vibration.Value;

            DataSettings.Vibration.OnValueChanged += SettingsVibrationValue_EventValueChanged;
        }

        static void SettingsVibrationValue_EventValueChanged(bool isOn)
        {
            Taptic.tapticOn = DataSettings.Vibration.Value;
        }

        #endregion

        #region SRDebug

        static void InitSRDebug()
        {
            SRDebug.Init();
            SRDebug.Instance.PanelVisibilityChanged += (isVisible) => { if (!isVisible && SketchEngineFactory.PopupDebug != null) PopupManager.Create(SketchEngineFactory.PopupDebug); };
        }

        #endregion
    }
}
