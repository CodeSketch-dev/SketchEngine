using System;

namespace SketchEngine.Core.MainThread
{
    interface IMainThreadDispatcher
    {
        void Init();
        void Enqueue(Action action);
    }
}
