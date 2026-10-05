using Sirenix.OdinInspector;
using UnityEngine;

using SketchEngine.SO;

namespace SketchEngine.Core
{
    public class SketchEngineFactory : ScriptableObjectSingleton<SketchEngineFactory>
    {
        [Title("Prefabs")]
        [SerializeField] GameObject _UINotificationText;
        [SerializeField] GameObject _popupDebug;
        [SerializeField] GameObject _internetPopup;

        static GameObject _UINotificationTextOverride;
        static GameObject _popupDebugOverride;
        static GameObject _internetPopupOverride;

        public static GameObject UINotificationText => _UINotificationTextOverride != null ? _UINotificationTextOverride : Instance._UINotificationText;
        public static GameObject PopupDebug => _popupDebugOverride != null ? _popupDebugOverride : Instance._popupDebug;
        public static GameObject InternetPopup => _internetPopupOverride != null ? _internetPopupOverride : Instance._internetPopup;

        public static void SetUINotificationTextOverride(GameObject prefab) => _UINotificationTextOverride = prefab;
        public static void SetPopupDebugOverride(GameObject prefab) => _popupDebugOverride = prefab;
        public static void SetInternetPopupOverride(GameObject prefab) => _internetPopupOverride = prefab;
    }
}
