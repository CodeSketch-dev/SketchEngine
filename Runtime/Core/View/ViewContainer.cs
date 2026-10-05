using System.Collections.Generic;
using SketchEngine.Core.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

using SketchEngine.Mono;
using SketchEngine.Core.Extensions.CSharp;
using SketchEngine.Diagnostics;
using UnityEngine.AddressableAssets;

namespace SketchEngine.UIView
{
    public class ViewContainer : MonoSingleton<ViewContainer>
    {
        protected override bool PersistAcrossScenes => true;

        readonly List<View> _views = new List<View>();

        bool _isTransiting = false;

        protected override void Awake()
        {
            base.Awake();

            SceneManager.activeSceneChanged += SceneManager_ActiveSceneChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneManager.activeSceneChanged -= SceneManager_ActiveSceneChanged;
        }

        void SceneManager_ActiveSceneChanged(Scene arg0, Scene arg1)
        {
            // Clear all view when scene changed
            for (int i = _views.Count - 1; i >= 0; i--)
                _views[i].Close();
        }

        View GetTopView()
        {
            return _views.Count <= 0 ? null : _views.Last();
        }

        void PopTopView()
        {
            _views.Pop();
        }

        void RevealTopView()
        {
            View topView = GetTopView();

            topView?.Reveal();
        }

        void BlockTopView()
        {
            View topView = GetTopView();

            topView?.Block();
        }

        public async UniTask<View> PushAsync(AssetReference viewAsset)
        {
            // Không cho push view mới khi đang có view khác transiting
            if (_isTransiting)
            {
                SketchDebug.Log<ViewContainer>($"Another View is transiting, can't push any new view {viewAsset}");
                return null;
            }

            _isTransiting = true;

            // try/finally đảm bảo _isTransiting LUÔN được reset dù load asset lỗi/throw -
            // thiếu cái này thì 1 lần load Addressable fail sẽ khóa cứng toàn bộ hệ thống View vĩnh viễn.
            try
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(viewAsset);

                await handle;

                if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"Failed to load View asset: {viewAsset}");
                    Addressables.Release(handle);
                    return null;
                }

                // Spawn view object từ asset vừa load
                View view = handle.Result.Create(TransformCached, false).GetComponent<View>();
                if (view == null)
                {
                    Debug.LogError($"Failed to get View component from loaded asset: {viewAsset}");
                    Addressables.Release(handle); // trước đây thiếu dòng này -> leak handle mỗi lần asset thiếu component View
                    return null;
                }

                view.GameObjectCached.SetActive(false);

                BlockTopView();

                // Handle view callback
                view.onCloseStart.AddListener(PopTopView);
                view.onCloseEnd.AddListener(() =>
                {
                    if (view && view.GameObjectCached)
                        Destroy(view.GameObjectCached);

                    Addressables.Release(handle);

                    RevealTopView();
                });

                view.Open();

                _views.Add(view);

                return view;
            }
            finally
            {
                _isTransiting = false;
            }
        }
    }
}
