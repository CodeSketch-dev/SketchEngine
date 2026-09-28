using Sirenix.OdinInspector;
using System;
using SketchEngine.Core.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SketchEngine.UIView
{
    public class ViewButtonOpen : UIButtonBase
    {
        [Title("Config")]
        [SerializeField] protected AssetReferenceGameObject _view;

        public event Action<View> eventViewOpened;

        public override async void Button_OnClick()
        {
            try
            {
                base.Button_OnClick();

                View view = await CodeSketchViewHelper.PushAsync(_view);

                eventViewOpened?.Invoke(view);

                OnViewOpened(view);
            }
            catch
            {
                //
            }
        }

        protected virtual void OnViewOpened(View view)
        {
        }
    }
}
