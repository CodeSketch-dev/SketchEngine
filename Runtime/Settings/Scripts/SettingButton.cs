using SketchEngine.Core.UI;
using UnityEngine;

namespace SketchEngine.Settings
{
    public class SettingButton : UIButtonBase
    {
        [SerializeField] GameObject _goEnable;
        [SerializeField] GameObject _goDisable;

        protected virtual bool IsOn { get; set; }

        protected override void Start()
        {
            base.Start();
            
            _goEnable.SetActive(IsOn);
            _goDisable.SetActive(!IsOn);
        }

        public override void Button_OnClick()
        {
            base.Button_OnClick();
            
            IsOn = !IsOn;
            
            _goEnable.SetActive(IsOn);
            _goDisable.SetActive(!IsOn);
        }
    }
}
