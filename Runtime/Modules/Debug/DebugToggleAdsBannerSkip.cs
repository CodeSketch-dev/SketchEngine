using SketchEngine.Data;

namespace SketchEngine.Debug
{
    public class DebugToggleAdsBannerSkip : DebugToggle
    {
        protected override bool IsOn 
        { 
            get => DataMaster.AdsBannerSkip.Value;
            set => DataMaster.AdsBannerSkip.Value = value;
        }
    }
}
