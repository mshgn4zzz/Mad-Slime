using System;
using MadSlime.Leaderboards.Data;
using MadSlime.Leaderboards.Interfaces;
using MadSlime.Saves.Interfaces;
using MadSlime.StateMachine.Logic;
using MadSlime.UI.Leaderboards.StateMachine.States;

namespace MadSlime.UI.Leaderboards.StateMachine
{
    public class LeaderboardStateMachine : BaseStateMachine<ILeaderboardState>, IDisposable
    {
        private readonly ILeaderboardService _service;
        private readonly WaitingForSdkState _waiting;
        private readonly LoadingLeaderboardState _loading;
        private readonly AuthRequiredLeaderboardState _auth = new AuthRequiredLeaderboardState();
        private readonly ReadyLeaderboardState _ready = new ReadyLeaderboardState();
        private readonly EmptyLeaderboardState _empty = new EmptyLeaderboardState();
        private readonly ErrorLeaderboardState _error = new ErrorLeaderboardState();
        private long _revision;

        public LeaderboardStateMachine(ISavesReadiness readiness, ILeaderboardService service, string name)
        {
            _service = service;
            _waiting = new WaitingForSdkState(readiness);
            _loading = new LoadingLeaderboardState(service, name);
            RegisterState(_waiting);
            RegisterState(_loading);
            RegisterState(_auth);
            RegisterState(_ready);
            RegisterState(_empty);
            RegisterState(_error);
            Subscribe();
        }

        public event Action LoadingStarted;
        public event Action AuthorizationRequired;
        public event Action<LeaderboardData> SnapshotPrepared;
        public event Action Prepared;
        public event Action EmptyResultReceived;
        public event Action Failed;

        public string PlayerId => _service.PlayerId;

        public void Start()
        {
            Stop();

            long revision = _revision;

            LoadingStarted?.Invoke();

            if (revision == _revision)
            {
                SwitchState<WaitingForSdkState>();
            }
        }

        public bool IsReady()
        {
            return CurrentState == _ready;
        }

        public override void Stop()
        {
            _revision++;
            base.Stop();
        }

        public void RequestAuthorization()
        {
            if (CurrentState == _auth && _service.IsAuthorized == false)
            {
                _service.OpenAuthDialog();
            }
        }

        public void Dispose()
        {
            Stop();
            Unsubscribe();
        }

        private void Subscribe()
        {
            _waiting.Prepared += OnSdkReady;
            _waiting.Failed += OnFailed;
            _loading.Loaded += OnLoaded;
            _loading.Failed += OnFailed;
            _auth.Entered += OnAuthRequired;
            _ready.Entered += OnReady;
            _empty.Entered += OnEmpty;
            _error.Entered += OnError;
        }

        private void Unsubscribe()
        {
            _waiting.Prepared -= OnSdkReady;
            _waiting.Failed -= OnFailed;
            _loading.Loaded -= OnLoaded;
            _loading.Failed -= OnFailed;
            _auth.Entered -= OnAuthRequired;
            _ready.Entered -= OnReady;
            _empty.Entered -= OnEmpty;
            _error.Entered -= OnError;
        }

        private void OnSdkReady()
        {
            if (CurrentState != _waiting)
            {
                return;
            }

            if (_service.IsAuthorized)
            {
                SwitchState<LoadingLeaderboardState>();
            }
            else
            {
                SwitchState<AuthRequiredLeaderboardState>();
            }
        }

        private void OnLoaded(LeaderboardData snapshot)
        {
            if (CurrentState != _loading)
            {
                return;
            }

            long revision = _revision;

            SnapshotPrepared?.Invoke(snapshot);

            if (revision != _revision || CurrentState != _loading)
            {
                return;
            }

            if (snapshot.IsPlayerError)
            {
                SwitchState<ErrorLeaderboardState>();
            }
            else if (snapshot.Players.Length == 0 && snapshot.CurrentPlayer.HasValue == false)
            {
                SwitchState<EmptyLeaderboardState>();
            }
            else
            {
                SwitchState<ReadyLeaderboardState>();
            }
        }

        private void OnFailed()
        {
            if (CurrentState == _loading || CurrentState == _waiting)
            {
                SwitchState<ErrorLeaderboardState>();
            }
        }

        private void OnAuthRequired()
        {
            AuthorizationRequired?.Invoke();
        }

        private void OnReady()
        {
            Prepared?.Invoke();
        }

        private void OnEmpty()
        {
            EmptyResultReceived?.Invoke();
        }

        private void OnError()
        {
            Failed?.Invoke();
        }
    }
}
