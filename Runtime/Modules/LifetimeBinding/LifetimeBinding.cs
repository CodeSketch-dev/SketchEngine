using System;
using UnityEngine;

namespace SketchEngine.Modules.Lifetime
{
    public class LifetimeBinding : MonoBehaviour
    {
        public event Action EventRelease;
        
        void OnDestroy()
        {
            EventRelease?.Invoke();
        }
    }
}
