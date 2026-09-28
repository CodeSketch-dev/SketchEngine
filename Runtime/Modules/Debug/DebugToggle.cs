using UnityEngine;

namespace SketchEngine.Debug
{
    public class DebugToggle : DebugButton
    {
        protected virtual bool IsOn { get; set; }

        protected override void Awake()
        {
            base.Awake();

            UpdateUI();
        }

        public override void Button_OnClick()
        {
            base.Button_OnClick();

            IsOn = !IsOn;

            UpdateUI();
        }

        void UpdateUI()
        {
            Button.image.color = IsOn ? Color.green : Color.red;
        }
    }
}
