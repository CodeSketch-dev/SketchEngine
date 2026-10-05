using SketchEngine.Core.UI;
using UnityEngine;

namespace SketchEngine.Settings
{
    public class SettingButton : UIButtonBase
    {
        [SerializeField] GameObject _goEnable;
        [SerializeField] GameObject _goDisable;

        protected virtual bool IsOn { get; set; }

        protected GameObject StateEnable => _goEnable;
        protected GameObject StateDisable => _goDisable;

        protected virtual void Start()
        {
            UpdateRendering();
        }

        public override void Button_OnClick()
        {
            base.Button_OnClick();

            IsOn = !IsOn;
            UpdateRendering();
        }

        protected virtual void UpdateRendering()
        {
            if (StateEnable)
                StateEnable.SetActive(IsOn);
            if (StateDisable)
                StateDisable.SetActive(!IsOn);
        }
    }
}
