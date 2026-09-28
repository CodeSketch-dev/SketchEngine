using SketchEngine.Mono;
using UnityEngine;

namespace SketchEngine.Core.MainThread
{
    class MainThreadDispatcherRuntime : MainThreadDispatcherBase
    {
        public override void Init()
        {
            MonoCallback.SafeInstance.EventUpdate += Tick;
        }
    }
}
