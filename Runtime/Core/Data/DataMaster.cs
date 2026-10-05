using UnityEngine;

namespace SketchEngine.Data
{
    public class DataMaster : DataBlock<DataMaster>
    {
        [SerializeField] DataValue<bool> _adsRewardedSkip;
        [SerializeField] DataValue<bool> _adsInterstitialSkip;
        [SerializeField] DataValue<bool> _adsBannerSkip;
        [SerializeField] DataValue<bool> _uiHidden;

        public static DataValue<bool> AdsRewardedSkip => INSTANCE._adsRewardedSkip;
        public static DataValue<bool> AdsInterstitialSkip => INSTANCE._adsInterstitialSkip;
        public static DataValue<bool> AdsBannerSkip => INSTANCE._adsBannerSkip;
        public static DataValue<bool> UIHidden => INSTANCE._uiHidden;

        protected override void Init()
        {
            base.Init();

            _uiHidden ??= new DataValue<bool>(false);
            _adsRewardedSkip ??= new DataValue<bool>(false);
            _adsInterstitialSkip ??= new DataValue<bool>(false);
            _adsBannerSkip ??= new DataValue<bool>(false);
        }
    }
}
