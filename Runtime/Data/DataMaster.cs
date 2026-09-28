using UnityEngine;

namespace SketchEngine.Data
{
    public class DataMaster : DataBlock<DataMaster>
    {
        [SerializeField] DataValue<bool> _adsRewardSkip;
        [SerializeField] DataValue<bool> _adsInterSkip;
        [SerializeField] DataValue<bool> _adsBannerSkip;
        [SerializeField] DataValue<bool> _uiHidden;
        
        public static DataValue<bool> AdsRewardSkip => Instance._adsRewardSkip;
        public static DataValue<bool> AdsInterSkip => Instance._adsInterSkip;
        public static DataValue<bool> AdsBannerSkip => Instance._adsBannerSkip;
        public static DataValue<bool> UIHidden => Instance._uiHidden;

        protected override void Init()
        {
            base.Init();
            
            _uiHidden ??= new DataValue<bool>(false);
            _adsRewardSkip ??= new DataValue<bool>(false);
            _adsInterSkip ??= new DataValue<bool>(false);
            _adsBannerSkip ??= new DataValue<bool>(false);
        }
    }
}
