using UnityEngine;

namespace SketchEngine.Utilities.Text
{
    public class UITextTimeCountdownMS : UITextTimeCountdown
    {
        static readonly string FORMAT = "{0:00}:{1:00}";

        [SerializeField] string _format;

        protected override void UpdateTimeDisplay(float timeToDisplay)
        {
            base.UpdateTimeDisplay(timeToDisplay);

            int time = Mathf.CeilToInt(timeToDisplay);

            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60);

            if (string.IsNullOrEmpty(_format))
                Text.text = string.Format(FORMAT, minutes, seconds);
            else
                Text.text = string.Format(_format, minutes, seconds);
        }
    }
}
