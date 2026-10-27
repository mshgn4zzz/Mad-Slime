using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game
{
    public class CompletedState : IGameplayState
    {
        private Pauser _pauser;
        private CancellationTokenSource _delaySource;
        private float _endDelay;

        public event Action Entered;

        public event Action FillRequested;

        public CompletedState(Pauser pauser)
        {
            _pauser = pauser;
        }

        public void Configure(float endDelay)
        {
            _endDelay = endDelay;
        }

        public void Enter()
        {
            if (_delaySource != null)
            {
                return;
            }

            _delaySource = new CancellationTokenSource();

            CancellationToken token = _delaySource.Token;

            _pauser.RequestPause();
            Entered?.Invoke();
            FinishDelayAsync(token).Forget();
        }

        public void Exit()
        {
            CancellationTokenSource source = _delaySource;

            if (source == null)
            {
                return;
            }

            _delaySource = null;
            source.Cancel();
            source.Dispose();
            _pauser.RequestResume();
        }

        private async UniTask FinishDelayAsync(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                if (_endDelay > 0f)
                {
                    await UniTask.Delay((int)(_endDelay * 1000f), DelayType.Realtime, cancellationToken: token);
                }

                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                return;
            }

            FillRequested?.Invoke();
        }
    }
}
