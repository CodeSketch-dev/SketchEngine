using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine
{
    public class MonoCachedHub<THub> : MonoCached where THub : Component
    {
        [SerializeField, HideInInspector] THub _hub;

        public THub Hub
        {
            get
            {
                if (_hub == null)
                    _hub = GetComponentInChildren<THub>();
                if (_hub == null)
                    _hub = GetComponentInParent<THub>();
                return _hub;
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!Application.isPlaying && _hub == null)
                _hub = GetComponentInParent<THub>();
        }
#endif
    }
}
