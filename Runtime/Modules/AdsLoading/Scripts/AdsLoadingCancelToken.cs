using System.Threading;

namespace SketchEngine.AdsLoading
{
    public class AdsLoadingCancelToken
    {
        CancellationTokenSource _cancelTokenSource;

        public CancellationToken Token
        {
            get
            {
                if (_cancelTokenSource == null)
                    _cancelTokenSource = new CancellationTokenSource();

                return _cancelTokenSource.Token;
            }
        }

        public void Cancel()
        {
            _cancelTokenSource?.Cancel();
            _cancelTokenSource?.Dispose();
            _cancelTokenSource = null;
        }
    }
}
