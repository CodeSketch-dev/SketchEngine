using UnityEngine;

namespace SketchEngine.Utilities.Text
{
    public class UITextTimeCountdownMS : UITextTimeCountdown
    {
        static readonly string s_format = "{0:00}:{1:00}";

        [SerializeField] string _textFormat;

        protected override void UpdateTimeDisplay(float timeToDisplay)
        {
            base.UpdateTimeDisplay(timeToDisplay);

            int time = Mathf.CeilToInt(timeToDisplay);

            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60);

            if (string.IsNullOrEmpty(_textFormat))
                Text.text = string.Format(s_format, minutes, seconds);
            else
                Text.text = string.Format(_textFormat, minutes, seconds);
        }
    }
}
