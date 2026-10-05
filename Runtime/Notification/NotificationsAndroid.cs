using System;
using UnityEngine;

#if SKETCHENGINE_NOTIFICATIONS && UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif

namespace Game
{
    public class NotificationsAndroid : MonoBehaviour
    {
#if SKETCHENGINE_NOTIFICATIONS && UNITY_ANDROID
        const string CHANNEL_ID = "default_channel";
#endif

        // =====================================================
        // PUBLIC API (ALWAYS VALID)
        // =====================================================

        public void Initialize()
        {
#if SKETCHENGINE_NOTIFICATIONS && UNITY_ANDROID
            RequestAuthorization();
            RegisterChannel();
#endif
            // khi không bật define → noop
        }

        public void Send(string identifier, string title, string body, int delaySeconds)
        {
#if SKETCHENGINE_NOTIFICATIONS && UNITY_ANDROID
            var notification = new AndroidNotification
            {
                Title = title,
                Text = body,
                FireTime = DateTime.Now.AddSeconds(delaySeconds),
                SmallIcon = "icon_0",
                LargeIcon = "icon_1"
            };

            AndroidNotificationCenter.SendNotificationWithExplicitID(
                notification,
                CHANNEL_ID,
                identifier.GetHashCode()
            );
#endif
            // noop nếu không bật SKETCHENGINE_NOTIFICATIONS
        }

        public void Cancel(string identifier)
        {
#if SKETCHENGINE_NOTIFICATIONS && UNITY_ANDROID
            AndroidNotificationCenter.CancelNotification(identifier.GetHashCode());
#endif
            // noop nếu không bật define
        }

        // =====================================================
        // ANDROID IMPLEMENTATION
        // =====================================================

#if SKETCHENGINE_NOTIFICATIONS && UNITY_ANDROID
        void RequestAuthorization()
        {
            if (!Permission.HasUserAuthorizedPermission(
                    "android.permission.POST_NOTIFICATIONS"))
            {
                Permission.RequestUserPermission(
                    "android.permission.POST_NOTIFICATIONS");
            }
        }

        void RegisterChannel()
        {
            var channel = new AndroidNotificationChannel
            {
                Id = CHANNEL_ID,
                Name = "Default",
                Importance = Importance.Default,
                Description = "Game Notifications"
            };

            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }
#endif
    }
}
