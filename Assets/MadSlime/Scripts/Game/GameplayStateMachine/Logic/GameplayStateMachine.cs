using System;
using System.Threading;
using Audio;
using Cysharp.Threading.Tasks;
using Leaderboards;

namespace Game
{
    public class GameplayStateMachine : BaseStateMachine<IGameplayState>, ISessionStateMachine, IDisposable
    {
        private WaitingForStartState _waiting;
        private PlayingState _playing;
        private CompletedState _completed;
        private bool _hasSession;
        private bool _isSessionFinished;
        private CollectedItemsCounter _collectedItems;
        private LeaderboardReporter _leaderboard;
        private Playlist _playlist;

        public event Action GameplayStarted;

        public event Action SessionFinished;

        public event Action FillRequested;

        public event Action<float> PreparationProgressChanged;

        public GameplayStateMachine(
            WaitingForStartState waiting,
            PlayingState playing,
            CompletedState completed,
            CollectedItemsCounter collectedItems,
            LeaderboardReporter leaderboard)
        {
            _waiting = waiting;
            _playing = playing;
            _completed = completed;
            _collectedItems = collectedItems;
            _leaderboard = leaderboard;
            RegisterState(waiting);
            RegisterState(playing);
            RegisterState(completed);
            _waiting.StartRequested += OnStartRequested;
            _waiting.PreparationProgressChanged += OnPreparationProgressChanged;
            _playing.Started += OnGameplayStarted;
            _playing.CompletionRequested += OnCompletionRequested;
            _completed.Entered += OnSessionFinished;
            _completed.FillRequested += OnFillRequested;
        }

        public void Configure(Playlist playlist, float endDelay, float finalSeconds)
        {
            ArgumentGuard.CheckNotNull(playlist, nameof(playlist));

            if (float.IsNaN(endDelay) || float.IsInfinity(endDelay) || endDelay < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(endDelay), "End delay must be finite and nonnegative.");
            }

            if (float.IsNaN(finalSeconds) || float.IsInfinity(finalSeconds) || finalSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(finalSeconds), "Final countdown must be finite and positive.");
            }

            _playlist = playlist;
            _playing.Configure(finalSeconds);
            _completed.Configure(endDelay);
        }

        public async UniTask PrepareAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_playlist == null)
            {
                throw new InvalidOperationException("GameplayStateMachine must be configured before preparation.");
            }

            Stop();
            _hasSession = true;
            _isSessionFinished = false;

            try
            {
                await _waiting.PrepareAsync(_playlist, cancellationToken);
            }
            catch
            {
                Stop();
                throw;
            }
        }

        public void Start()
        {
            if (_hasSession && CurrentState == null)
            {
                SwitchState<WaitingForStartState>();
            }
        }

        public override void Stop()
        {
            base.Stop();

            if (_hasSession)
            {
                _hasSession = false;
                _waiting.Cleanup();
                OnSessionFinished();
            }
        }

        public void PauseByRequest()
        {
            switch (CurrentState)
            {
                case WaitingForStartState waiting when waiting == _waiting:
                    waiting.PauseByRequest();
                    break;

                case PlayingState playing when playing == _playing:
                    playing.PauseByRequest();
                    break;
            }
        }

        public void ResumeByRequest()
        {
            switch (CurrentState)
            {
                case WaitingForStartState waiting when waiting == _waiting:
                    waiting.ResumeByRequest();
                    break;

                case PlayingState playing when playing == _playing:
                    playing.ResumeByRequest();
                    break;
            }
        }

        public void Dispose()
        {
            Stop();
            _waiting.StartRequested -= OnStartRequested;
            _waiting.PreparationProgressChanged -= OnPreparationProgressChanged;
            _playing.Started -= OnGameplayStarted;
            _playing.CompletionRequested -= OnCompletionRequested;
            _completed.Entered -= OnSessionFinished;
            _completed.FillRequested -= OnFillRequested;
        }

        private void OnStartRequested()
        {
            if (CurrentState == _waiting)
            {
                SwitchState<PlayingState>();
            }
        }

        private void OnCompletionRequested()
        {
            if (CurrentState == _playing)
            {
                SwitchState<CompletedState>();
            }
        }

        private void OnGameplayStarted()
        {
            GameplayStarted?.Invoke();
        }

        private void OnFillRequested()
        {
            FillRequested?.Invoke();
        }

        private void OnPreparationProgressChanged(float progress)
        {
            PreparationProgressChanged?.Invoke(progress);
        }

        private void OnSessionFinished()
        {
            if (_isSessionFinished == false)
            {
                _isSessionFinished = true;
                _collectedItems.CommitSession();
                _leaderboard.Report();
                SessionFinished?.Invoke();
            }
        }
    }
}
