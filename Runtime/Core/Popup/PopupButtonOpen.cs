using Sirenix.OdinInspector;
using System;
using SketchEngine.Core.UI;
using UnityEngine;

namespace SketchEngine.UIPopup
{
    public class PopupButtonOpen : UIButtonBase
    {
        [Title("Config")]
        [SerializeField] GameObject _popup;

        public event Action<Popup> eventSpawnPopup;

        public override void Button_OnClick()
        {
            base.Button_OnClick();

            SpawnPopup();
        }

        protected virtual void HandleSpawnPopup(Popup popupBehaviour)
        {
        }

        public Popup SpawnPopup()
        {
            Popup popup = PopupManager.Create(_popup);
            if (popup == null) return null;

            eventSpawnPopup?.Invoke(popup);

            HandleSpawnPopup(popup);

            return popup;
        }
    }
}
