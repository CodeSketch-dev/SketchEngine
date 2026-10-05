using SketchEngine.Data;

namespace SketchEngine.Debug
{
    public class DebugToggleAdsInterSkip : DebugToggle
    {
        protected override bool IsOn
        {
            get => DataMaster.AdsInterstitialSkip.Value;
            set => DataMaster.AdsInterstitialSkip.Value = value;
        }
    }
}
