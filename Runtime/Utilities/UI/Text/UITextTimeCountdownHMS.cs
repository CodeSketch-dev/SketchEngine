using UnityEngine;

namespace SketchEngine.Utilities.Text
{
    public class UITextTimeCountdownHMS : UITextTimeCountdown
    {
        [SerializeField] string _textFormat;

        static readonly string s_format = "{0:00}:{1:00}:{2:00}";

        protected override void UpdateTimeDisplay(float timeToDisplay)
        {
            base.UpdateTimeDisplay(timeToDisplay);

            if (timeToDisplay == 0f)
            {
                Text.text = string.Format(s_format, 0, 0, 0);
                return;
            }

            timeToDisplay += 1;

            int hours = Mathf.FloorToInt(timeToDisplay / 3600f);
            int minutes = Mathf.FloorToInt((timeToDisplay - hours * 3600f) / 60f);
            int seconds = Mathf.FloorToInt(timeToDisplay % 60f);

            if (!string.IsNullOrEmpty(_textFormat))
                Text.text = string.Format(_textFormat, hours, minutes, seconds);
            else
                Text.text = string.Format(s_format, hours, minutes, seconds);
        }
    }
}
