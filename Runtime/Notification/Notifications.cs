using SketchEngine.Mono;
using UnityEngine;

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

        // =====================================================
        // LIFECYCLE
        // =====================================================

        protected void Start()
        {
#if SKETCHENGINE_NOTIFICATIONS && UNITY_ANDROID
            if (_android != null)
                _android.Initialize();
#endif
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                CancelAll();
        }

        // =====================================================
        // PUBLIC API (ALWAYS VALID)
        // =====================================================

        public static void ScheduleNotification(NotificationData data)
        {
            if (HasInstance)
                SafeInstance.Schedule(data);
        }

        public void Schedule(NotificationData data)
        {
#if SKETCHENGINE_NOTIFICATIONS && UNITY_ANDROID
            if (_android != null)
            {
                _android.Cancel(data.Id);
                _android.Send(data.Id, data.Title, data.Body, data.DelaySeconds);
            }
#endif
            // Không có package Mobile Notifications -> noop
        }

        public void CancelAll()
        {
#if SKETCHENGINE_NOTIFICATIONS && UNITY_ANDROID
            Unity.Notifications.Android.AndroidNotificationCenter.CancelAllNotifications();
#endif
        }
    }
}
