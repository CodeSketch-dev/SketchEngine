using UnityEngine;

namespace SketchEngine.Patterns.ChainOfResponsibility
{
    public abstract class ChainBase<T> : IChainHandler<T>
    {
        protected IChainHandler<T> _nextHandler;

        public IChainHandler<T> NextHandler => _nextHandler;

        public void SetNext(IChainHandler<T> nextHandler)
        {
            _nextHandler = nextHandler;
        }

        public abstract bool CanHandle(T request);

        public void Handle(T request)
        {
            if (CanHandle(request))
            {
                ProcessRequest(request);
            }
            else
            {
                _nextHandler?.Handle(request);
            }
        }

        public void PostProcess(T request)
        {
            if (CanHandle(request))
            {
                PostProcessRequest(request);
            }
            else
            {
                _nextHandler?.PostProcess(request);
            }
        }

        protected abstract void ProcessRequest(T request);
        protected abstract void PostProcessRequest(T request);
    }
}
