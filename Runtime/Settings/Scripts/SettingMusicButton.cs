namespace SketchEngine.Settings
{
    public class SettingMusicButton : SettingButton
    {
        protected override bool IsOn
        {
            get => DataSettings.MusicVolume.Value > 0.0f;
            set => DataSettings.MusicVolume.Value = value ? 1.0f : 0.0f;
        }
    }
}
