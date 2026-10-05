using Cysharp.Threading.Tasks;
using SketchEngine.Diagnostics;
using UnityEngine.AddressableAssets;

namespace SketchEngine.UIView
{
    public static class SketchEngineViewHelper
    {
        public static UniTask<View> PushAsync(AssetReference viewAsset)
        {
            // ViewContainer.Instance có thể null nếu gọi quá sớm (chưa có singleton trong scene) hoặc
            // quá trễ (đã bị destroy lúc OnApplicationQuit/đổi scene) -> tránh NullReferenceException.
            var container = ViewContainer.Instance;
            if (container == null)
            {
                SketchDebug.LogError<ViewContainer>($"ViewContainer.Instance is null, can't push view {viewAsset}");
                return UniTask.FromResult<View>(null);
            }

            return container.PushAsync(viewAsset);
        }
    }
}