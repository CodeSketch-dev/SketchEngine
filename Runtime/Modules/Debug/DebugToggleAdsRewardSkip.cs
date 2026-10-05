using SketchEngine.Data;

namespace SketchEngine.Debug
{
    public class DebugToggleAdsRewardSkip : DebugToggle
    {
        protected override bool IsOn
        {
            get => DataMaster.AdsRewardedSkip.Value;
            set => DataMaster.AdsRewardedSkip.Value = value;
        }
    }
}