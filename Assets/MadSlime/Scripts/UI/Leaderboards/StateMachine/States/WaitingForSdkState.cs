using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Saves.Interfaces;

namespace MadSlime.UI.Leaderboards.StateMachine.States
{
    public class WaitingForSdkState : ILeaderboardState
    {
        private readonly ISavesReadiness _readiness;
        private CancellationTokenSource _source;

        public WaitingForSdkState(ISavesReadiness readiness)
        {
            _readiness = readiness;
        }

        public event Action Prepared;
        public event Action Failed;

        public void Enter()
        {
            _source = new CancellationTokenSource();

            if (_readiness.IsReady)
            {
                Prepared?.Invoke();
                return;
            }

            WaitAsync(_source.Token).Forget();
        }

        public void Exit()
        {
            CancellationTokenSource source = _source;

            _source = null;
            source?.Cancel();
            source?.Dispose();
        }

        private bool IsReady()
        {
            return _readiness.IsReady;
        }

        private async UniTask WaitAsync(CancellationToken token)
        {
            try
            {
                await UniTask.WaitUntil(IsReady, cancellationToken: token)
                    .Timeout(TimeSpan.FromSeconds(8), DelayType.Realtime);
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                return;
            }
            catch (Exception)
            {
                if (token.IsCancellationRequested == false)
                {
                    Failed?.Invoke();
                }

                return;
            }

            Prepared?.Invoke();
        }
    }
}
