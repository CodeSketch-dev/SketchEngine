using System;
using UnityEngine;
using UnityEngine.UI;

namespace SketchEngine.Internet
{
    // Gắn lên prefab popup internet. Có 2 state: Warning (mất mạng, có nút Retry) và Reconnecting (đang kiểm tra).
    // Internet tự tìm component này sau khi tạo popup, không cần settings.
    public class InternetPopupView : MonoBehaviour
    {
        [SerializeField] GameObject _stateWarning;
        [SerializeField] GameObject _stateReconnecting;
        [SerializeField] Button _btnRetry;

        public event Action OnRetry;

        void Awake()
        {
            if (_btnRetry != null)
                _btnRetry.onClick.AddListener(HandleRetryClicked);
        }

        void OnDestroy()
        {
            if (_btnRetry != null)
                _btnRetry.onClick.RemoveListener(HandleRetryClicked);
        }

        public void ShowWarning()
        {
            SetState(warning: true);
            SetRetryInteractable(true);
        }

        public void ShowReconnecting()
        {
            SetState(warning: false);
            SetRetryInteractable(false);
        }

        public void SetRetryInteractable(bool interactable)
        {
            if (_btnRetry != null)
                _btnRetry.interactable = interactable;
        }

        void SetState(bool warning)
        {
            if (_stateWarning != null) _stateWarning.SetActive(warning);
            if (_stateReconnecting != null) _stateReconnecting.SetActive(!warning);
        }

        void HandleRetryClicked()
        {
            OnRetry?.Invoke();
        }
    }
}
