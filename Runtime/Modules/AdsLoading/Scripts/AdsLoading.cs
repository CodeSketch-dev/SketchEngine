using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SketchEngine.AdsLoading
{
    public class AdsLoading : AdsLoadingSingleton<AdsLoading>
    {
        protected override bool PersistAcrossScenes => true;

        [SerializeField] float _displayDuration = 2.0f;
        
        [SerializeField] GameObject _fullRect;
        [SerializeField] GameObject _smallRect;

        AdsLoadingCancelToken _cancelToken;

        public virtual bool IsAdsBreakSmallRect { get; set; } = true;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            _cancelToken?.Cancel();
        }

        public static void Show(Action callback = null)
        {
            if (!HasInstance) return;
            SafeInstance.Show_Internal(callback);
        }

        void Show_Internal(Action actionComplete)
        {
            if (IsAdsBreakSmallRect)
            {
                _smallRect.SetActive(true);
                _fullRect.SetActive(false);
            }
            else
            {
                _smallRect.SetActive(false);
                _fullRect.SetActive(true);
            }

            _cancelToken?.Cancel();
            _cancelToken = new AdsLoadingCancelToken();
            
            Task(actionComplete).AttachExternalCancellation(_cancelToken.Token);
        }

        async UniTask Task(Action callback)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_displayDuration));
            
            _smallRect.SetActive(false);
            _fullRect.SetActive(false);
            callback?.Invoke();
        }
    }
}