using SketchEngine.Mono;
using UnityEngine;
using UnityEngine.Serialization;

#if CODESKETCH_NOTIFICATIONS && UNITY_ANDROID
using Unity.Notifications.Android;
#endif

#if CODESKETCH_NOTIFICATIONS && (UNITY_IOS || UNITY_IPHONE)
using Unity.Notifications.iOS;
#endif

namespace Game
{
    [System.Serializable]
    public struct NotificationData
    {
        public string Id;
        public string Title;
        public string Body;
        public string Subtitle;
        public int DelaySeconds;
    }

    public class Notifications : MonoSingleton<Notifications>
    {
        protected override bool PersistAcrossScenes => true;

        [Header("Platform Handlers")]
        [SerializeField] NotificationsAndroid _android;
        [SerializeField] NotificationsiOS _ios;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        protected override void Start()
        {
            base.Start();

#if CODESKETCH_NOTIFICATIONS && UNITY_ANDROID
            if (_android != null)
                _android.Initialize();
#endif

#if CODESKETCH_NOTIFICATIONS && (UNITY_IOS || UNITY_IPHONE)
            if (_ios != null)
                StartCoroutine(_ios.RequestAuthorization());
#endif
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // Schedule notifications when app is not focused
            }
            else
            {
                CancelAll();
            }
        }

        // =====================================================
        // PUBLIC API (ALWAYS VALID)
        // =====================================================

        public static void ScheduleNotification(NotificationData data)
        {
            // API luôn tồn tại, không phụ thuộc define
            if (HasInstance)
                SafeInstance.Schedule(data);
        }

        public void Schedule(NotificationData data)
        {
#if CODESKETCH_NOTIFICATIONS && UNITY_ANDROID
            if (_android != null)
            {
                _android.Cancel(data.Id);
                _android.Send(
                    data.Id,
                    data.Title,
                    data.Body,
                    data.DelaySeconds
                );
            }
#endif

#if CODESKETCH_NOTIFICATIONS && (UNITY_IOS || UNITY_IPHONE)
            if (_ios != null)
            {
                _ios.Cancel(data.type.ToString());
                _ios.Send(
                    data.type.ToString(),
                    data.title,
                    data.body,
                    data.subtitle,
                    data.delaySeconds
                );
            }
#endif
            // Khi không bật CODESKETCH_NOTIFICATIONS → noop
        }

        public void CancelAll()
        {
#if CODESKETCH_NOTIFICATIONS && UNITY_ANDROID
            AndroidNotificationCenter.CancelAllNotifications();
#endif

#if CODESKETCH_NOTIFICATIONS && (UNITY_IOS || UNITY_IPHONE)
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
            // Khi không bật define → noop
        }
    }
}
