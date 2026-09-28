using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace SketchEngine.UIView
{
    public static class CodeSketchViewHelper
    {
        public static UniTask<View> PushAsync(AssetReference viewAsset)
        {
            return ViewContainer.Instance.PushAsync(viewAsset);
        }
    }
}