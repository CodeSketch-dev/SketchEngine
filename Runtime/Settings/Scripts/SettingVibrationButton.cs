namespace SketchEngine.Settings
{
    public class SettingVibrationButton : SettingButton
    {
        protected override bool IsOn
        {
            get => DataSettings.Vibration.Value;
            set => DataSettings.Vibration.Value = value;
        }
    }
}
