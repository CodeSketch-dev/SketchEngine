using SketchEngine.Core.Async;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SketchEngine.Optimize
{
    public class OptCanvasLayout : MonoBehaviour
    {
        [SerializeField] LayoutGroup _layoutGroup;
        [SerializeField] int _waitForFrameCount = 1;

        CancelToken _cancelToken;
        
        LayoutGroup LayoutGroup
        {
            get
            {
                if (_layoutGroup == null)
                    _layoutGroup = GetComponentInChildren<LayoutGroup>();
                return _layoutGroup;
            }
        }

        void OnDestroy()
        {
            _cancelToken?.Cancel();
        }

        void OnEnable()
        {
            HandleOptimize();
        }

        void OnDisable()
        {
            _cancelToken?.Cancel();
            LayoutGroup.enabled = false;
        }

        public void HandleOptimize()
        {
            _cancelToken?.Cancel();
            _cancelToken = new CancelToken();
            Task().AttachExternalCancellation(_cancelToken.Token);
        }

        async UniTask Task()
        {
            LayoutGroup.enabled = true;

            await UniTask.DelayFrame(_waitForFrameCount);
            
            LayoutGroup.enabled = false;
        }

        protected virtual void OnValidate()
        {
            if (_layoutGroup == null) _layoutGroup = LayoutGroup;
        }
    }
}
