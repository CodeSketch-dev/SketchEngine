using Sirenix.OdinInspector;
using UnityEngine;

using SketchEngine.SO;

namespace SketchEngine.Core
{
    public class CodeSketchFactory : ScriptableObjectSingleton<CodeSketchFactory>
    {
        [Title("Prefabs")]
        [SerializeField] GameObject _UINotificationText;
        [SerializeField] GameObject _popupDebug;

        static GameObject _UINotificationTextOverride;
        static GameObject _popupDebugOverride;

        public static GameObject UINotificationText => _UINotificationTextOverride != null ? _UINotificationTextOverride : Instance._UINotificationText;
        public static GameObject PopupDebug => _popupDebugOverride != null ? _popupDebugOverride : Instance._popupDebug;

        public static void SetUINotificationTextOverride(GameObject prefab) => _UINotificationTextOverride = prefab;
        public static void SetPopupDebugOverride(GameObject prefab) => _popupDebugOverride = prefab;
    }
}
