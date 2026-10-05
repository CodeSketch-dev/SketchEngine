using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SketchEngine.Core;
using SketchEngine.Diagnostics;
using SketchEngine.UIPopup;
using UnityEngine;
using UnityEngine.Networking;

namespace SketchEngine.Internet
{
    // Theo dõi mạng nền. Khi mất mạng: mở popup (state Warning có nút Retry).
    // Khi đang mở popup, vòng theo dõi không check nữa; chỉ nút Retry mới check (state Reconnecting).
    //
    // Application.internetReachability chỉ báo có adapter mạng hay không. Có adapter vẫn có thể không ra
    // internet (captive portal, router mất WAN), nên khi có adapter mới xác nhận bằng request nhẹ.
    public static class Internet
    {
        const string PrimaryUrl = "https://captive.apple.com/hotspot-detect.html";
        const string FallbackUrl = "https://1.1.1.1/cdn-cgi/trace";
        const int RequestTimeoutSeconds = 5;
        const float OnlineCheckInterval = 15f;
        const float MinReconnectingSeconds = 5f;

        public static bool IsEnabled { get; private set; }

        // Game gán nếu muốn bỏ qua kiểm tra (vd user đã mua remove ads). Trả true = coi như online.
        public static Func<bool> SkipCheck;

        static Popup s_popup;
        static InternetPopupView s_view;
        static bool s_checking;
        static CancellationTokenSource s_cts;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            s_cts?.Cancel();
            s_cts?.Dispose();
            s_cts = new CancellationTokenSource();

            MonitorAsync(s_cts.Token).Forget();
        }

        public static bool IsPopupShowing()
        {
            return s_popup != null;
        }

        public static void ForceClosePopup()
        {
            ClosePopup();
        }

        static async UniTaskVoid MonitorAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(OnlineCheckInterval), DelayType.UnscaledDeltaTime, cancellationToken: ct);

                    // Popup đang mở hoặc đang kiểm tra: không check thêm, để nút Retry lo.
                    if (IsPopupShowing() || s_checking) continue;

                    if (SkipCheck != null && SkipCheck())
                    {
                        IsEnabled = true;
                        continue;
                    }

                    bool online = await CheckOnlineAsync(ct);
                    IsEnabled = online;

                    if (!online)
                        OpenPopup();
                }
            }
            catch (OperationCanceledException)
            {
                // Dừng bình thường khi đổi scene hoặc thoát app.
            }
        }

        static void OnRetryClicked()
        {
            if (s_cts == null || s_cts.IsCancellationRequested) return;
            RetryAsync(s_cts.Token).Forget();
        }

        static async UniTaskVoid RetryAsync(CancellationToken ct)
        {
            if (s_checking) return;
            s_checking = true;

            try
            {
                if (s_view != null) s_view.ShowReconnecting();

                // Kiểm tra mạng có thể xong rất nhanh (không có adapter). Giữ trạng thái Reconnecting tối thiểu
                // một khoảng để người chơi thấy có phản hồi khi bấm Retry.
                UniTask<bool> checkTask = CheckOnlineAsync(ct);
                await UniTask.Delay(TimeSpan.FromSeconds(MinReconnectingSeconds), DelayType.UnscaledDeltaTime, cancellationToken: ct);
                bool online = await checkTask;

                IsEnabled = online;

                if (online)
                    ClosePopup();
                else if (s_view != null)
                    s_view.ShowWarning();
            }
            catch (OperationCanceledException)
            {
                // Thoát app hoặc đổi scene giữa chừng.
            }
            finally
            {
                s_checking = false;
            }
        }

        static async UniTask<bool> CheckOnlineAsync(CancellationToken ct)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return false;

            if (await PingAsync(PrimaryUrl, ct))
                return true;

            return await PingAsync(FallbackUrl, ct);
        }

        static async UniTask<bool> PingAsync(string url, CancellationToken ct)
        {
            // GET thay vì HEAD: một số endpoint không hỗ trợ HEAD. Body nhỏ nên không tốn nhiều.
            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = RequestTimeoutSeconds;

                try
                {
                    await request.SendWebRequest().ToUniTask(cancellationToken: ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    return false;
                }

                return request.result == UnityWebRequest.Result.Success;
            }
        }

        static void OpenPopup()
        {
            if (IsPopupShowing())
            {
                if (s_view != null) s_view.ShowWarning();
                return;
            }

            GameObject prefab = SketchEngineFactory.InternetPopup;
            if (prefab == null)
            {
                SketchDebug.LogWarning(typeof(Internet), "Chưa gán InternetPopup trong SketchEngineFactory.");
                return;
            }

            s_popup = PopupManager.Create(prefab);
            if (s_popup == null) return;

            s_view = s_popup.GetComponent<InternetPopupView>();
            if (s_view == null)
            {
                SketchDebug.LogWarning(typeof(Internet), "Popup internet thiếu component InternetPopupView.");
                return;
            }

            s_view.OnRetry += OnRetryClicked;
            s_view.ShowWarning();
        }

        static void ClosePopup()
        {
            if (s_view != null)
                s_view.OnRetry -= OnRetryClicked;

            s_view = null;

            if (s_popup != null)
                s_popup.Close();

            s_popup = null;
        }
    }
}
