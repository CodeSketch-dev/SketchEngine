using UnityEngine;

namespace SketchEngine.Utilities
{
    /// <summary>
    /// Add this component to any object and it'll set the target frame rate and vsync count. Note that vsync count must be 0 for the target FPS to work.
    /// </summary>
    public class FpsUnlock : MonoBehaviour
    {
        /// the target FPS you want the game to run at
        public int TargetFPS = 60;
        [Range(0, 2)] public int VSyncCount = 0;

        /// <summary>
        /// On start we change our target fps and vsync settings
        /// </summary>
        protected virtual void Start()
        {
            UpdateSettings();
        }

        /// <summary>
        /// When a value gets changed in the editor, we update our settings
        /// </summary>
        protected virtual void OnValidate()
        {
            UpdateSettings();
        }

        /// <summary>
        /// Updates the target frame rate value and vsync count setting
        /// </summary>
        protected virtual void UpdateSettings()
        {
            QualitySettings.vSyncCount = VSyncCount;
            Application.targetFrameRate = TargetFPS;
        }
    }
}
