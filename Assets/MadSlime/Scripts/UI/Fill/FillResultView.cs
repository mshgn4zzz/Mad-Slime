using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MadSlime.UI.Fill
{
    public class FillResultView : IDisposable
    {
        private CancellationTokenSource _source;

        public async UniTask WaitForResultAsync(float seconds, CancellationToken lifetime)
        {
            Cancel();

            CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(lifetime);

            _source = source;

            CancellationToken token = source.Token;

            try
            {
                if (seconds > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.Realtime, cancellationToken: token);
                }

                token.ThrowIfCancellationRequested();
            }
            finally
            {
                if (ReferenceEquals(_source, source))
                {
                    _source = null;
                }

                source.Dispose();
            }
        }

        public void Cancel()
        {
            CancellationTokenSource source = _source;

            _source = null;
            source?.Cancel();
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}
