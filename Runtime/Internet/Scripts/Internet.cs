using System;
using System.Threading;
using SketchEngine.Core.Async;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using SketchEngine.Diagnostics;
using SketchEngine.UIPopup;

namespace SketchEngine.Internet
{
    public static class Internet
    {
#if CODESKETCH_INTERNET
        public static bool IsEnabled { get; private set; }

        static Popup InternetPopup;
        static CancelToken CancelToken;

        // ================= INIT =================

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            StartCheckLoop();
        }

        // ================= LOOP =================

        static void StartCheckLoop()
        {
            CancelToken?.Cancel();
            CancelToken = new CancelToken();

            CheckLoop(CancelToken.Token)
                .AttachExternalCancellation(CancelToken.Token);
        }

        static async UniTask CheckLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                bool hasInternet = await CheckOnce(token);

                if (hasInternet)
                {
                    ClosePopup();

                    // ✅ ONLINE → nghỉ theo interval
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(InternetSettings.CheckInterval),
                        cancellationToken: token
                    );
                }
                else
                {
                    OpenPopup();

                    // ❗ OFFLINE + POPUP → request tiếp NGAY
                    // Không delay, không await frame
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
        }

        // ================= CHECK =================

        static async UniTask<bool> CheckOnce(CancellationToken token)
        {
            try
            {
                using var request = UnityWebRequest.Get(InternetSettings.Url);
                request.timeout = InternetSettings.RequestTimeout;

                await request.SendWebRequest().WithCancellation(token);

                IsEnabled = request.result == UnityWebRequest.Result.Success;
            }
            catch
            {
                IsEnabled = false;
            }

            return IsEnabled;
        }

        // ================= POPUP =================

        static void OpenPopup()
        {
            if (InternetPopup != null)
                return;

            InternetPopup = PopupManager.Create(InternetSettings.Popup);
            CodeSketchDebug.LogRuntime(
                "[Internet] Offline → popup opened",
                Color.yellow
            );
        }

        static void ClosePopup()
        {
            if (InternetPopup == null)
                return;

            InternetPopup.Close();
            InternetPopup = null;

            CodeSketchDebug.LogRuntime(
                "[Internet] Online → popup closed",
                Color.green
            );
        }

        // ================= PUBLIC API =================

        public static bool IsPopupShowing()
        {
            return InternetPopup != null;
        }

        public static void ForceClosePopup()
        {
            ClosePopup();
        }

#else
        public static bool IsEnabled { get; private set; }
#endif
    }
}
