using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Leaderboards.Data;
using MadSlime.Leaderboards.Interfaces;

namespace MadSlime.UI.Leaderboards.StateMachine.States
{
    public class LoadingLeaderboardState : ILeaderboardState
    {
        private readonly ILeaderboardService _service;
        private readonly string _name;
        private CancellationTokenSource _source;

        public LoadingLeaderboardState(ILeaderboardService service, string name)
        {
            _service = service;
            _name = name;
        }

        public event Action<LeaderboardData> Loaded;
        public event Action Failed;

        public void Enter()
        {
            _source = new CancellationTokenSource();
            LoadAsync(_source.Token).Forget();
        }

        public void Exit()
        {
            CancellationTokenSource source = _source;

            _source = null;
            source?.Cancel();
            source?.Dispose();
        }

        private async UniTask LoadAsync(CancellationToken token)
        {
            LeaderboardData snapshot;

            try
            {
                snapshot = await _service.RequestEntriesAsync(_name, 10, token);
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

            Loaded?.Invoke(snapshot);
        }
    }
}
