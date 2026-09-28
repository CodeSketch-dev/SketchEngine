namespace SketchEngine.Settings
{
    public class SettingSoundButton : SettingButton
    {
        protected override bool IsOn
        { 
            get => DataSettings.SoundVolume.Value > 0.0f;
            set => DataSettings.SoundVolume.Value = value ? 1.0f : 0.0f;
        }
    }
}
