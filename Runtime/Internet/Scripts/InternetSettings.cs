using SketchEngine.SO;
using UnityEngine;
using UnityEngine.Serialization;

namespace SketchEngine.Internet
{
    public class InternetSettings : ScriptableObjectSingleton<InternetSettings>
    {
        [Header("Check")]
        [Tooltip("URL để ping (HTTP GET).")]
        [SerializeField] string _url = "https://www.google.com";

        [Min(1)]
        [Tooltip("Timeout (giây) cho mỗi request.")]
        [SerializeField] int _requestTimeout = 5;

        [Min(1f)]
        [Tooltip("Chu kỳ kiểm tra (giây) – dùng cho Internet_Checker loop.")]
        [SerializeField] float _checkInterval = 3f;

        [Header("Popup")] 
        [SerializeField] GameObject _popup;

        public static string Url => Instance._url;
        public static int RequestTimeout => Instance._requestTimeout;
        public static float CheckInterval => Instance._checkInterval;
        public static GameObject Popup => Instance._popup;
    }
}