using UnityEngine;

namespace SketchEngine.Patterns.ChainOfResponsibility
{
    public class ChainManager<T>
    {
        IChainHandler<T> _firstHandler;

        public void AddHandler(IChainHandler<T> handler)
        {
            if (_firstHandler == null)
            {
                _firstHandler = handler;
            }
            else
            {
                var current = _firstHandler;
                while (current is ChainBase<T> baseHandler && baseHandler.NextHandler != null)
                {
                    current = baseHandler.NextHandler;
                }
                
                current.SetNext(handler);
            }
        }
        
        public void Process(T request)
        {
            _firstHandler?.Handle(request); // Bắt đầu xử lý từ handler đầu tiên
        }

        public void PostProcess(T request)
        {
            _firstHandler?.PostProcess(request);
        }
    }
}
