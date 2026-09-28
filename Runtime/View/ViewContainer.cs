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
            // Can't push another view when it is transiting
            if (_isTransiting)
            {
                CodeSketchDebug.Log<ViewContainer>($"Another View is transiting, can't push any new view {viewAsset}");
                return null;
            }

            // Set transiting flag
            _isTransiting = true;

            // Wait new view to be loaded
            var handle = Addressables.LoadAssetAsync<GameObject>(viewAsset);

            await handle;

            // Spawn view object from loaded asset
            View view = handle.Result.Create(TransformCached, false).GetComponent<View>();
            if (view == null)
            {
                Debug.LogError($"Failed to get View component from loaded asset: {viewAsset}");
                _isTransiting = false;
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

                // Release asset after use
                //handle.Release();
                Addressables.Release(handle);

                RevealTopView();
            });

            // Open new view
            view.Open();

            // Push new view into stack
            _views.Add(view);

            // Unset transiting flag
            _isTransiting = false;

            return view;
        }
    }
}
