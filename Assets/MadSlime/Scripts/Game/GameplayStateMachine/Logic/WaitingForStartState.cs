using System;
using System.Threading;
using Audio;
using Cysharp.Threading.Tasks;
using Levels;
using PlayerInput;
using Quota;
using Saves;

namespace Game
{
    public class WaitingForStartState : IGameplayState
    {
        private PlayerInputReader _inputReader;
        private LevelConfigResolver _configResolver;
        private ILevelStorage _levelStorage;
        private LevelGenerator _levelGenerator;
        private Timer _timer;
        private QuotaCounter _quotaCounter;
        private GameplayPlayerSetup _playerSetup;
        private GameplayActivityController _activity;
        private FinalCountdownEffects _effects;
        private Pauser _pauser;
        private IMusicPlayer _musicPlayer;
        private Playlist _playlist;
        private bool _isEntered;
        private bool _hasWaitingPause;
        private int _pauseRequests;

        public event Action StartRequested;

        public event Action<float> PreparationProgressChanged;

        public WaitingForStartState(
            PlayerInputReader inputReader,
            LevelConfigResolver configResolver,
            ILevelStorage levelStorage,
            LevelGenerator levelGenerator,
            Timer timer,
            QuotaCounter quotaCounter,
            GameplayPlayerSetup playerSetup,
            GameplayActivityController activity,
            FinalCountdownEffects effects,
            Pauser pauser,
            IMusicPlayer musicPlayer)
        {
            _inputReader = inputReader;
            _configResolver = configResolver;
            _levelStorage = levelStorage;
            _levelGenerator = levelGenerator;
            _timer = timer;
            _quotaCounter = quotaCounter;
            _playerSetup = playerSetup;
            _activity = activity;
            _effects = effects;
            _pauser = pauser;
            _musicPlayer = musicPlayer;
        }

        public async UniTask PrepareAsync(Playlist playlist, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _playlist = playlist;
            _inputReader.enabled = false;
            _activity.SetRunning(false);
            _effects.Stop();
            _playerSetup.PrepareSession();

            LevelConfig config = _configResolver.GetConfigFor(_levelStorage.CurrentLevel);

            _timer.Setup(config.TimerDuration);
            await _levelGenerator.GenerateAsync(ReportGenerationProgress, cancellationToken);
            await _playerSetup.PrepareOutlineAsync(_levelGenerator.SpawnedItems, ReportOutlineProgress, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            _quotaCounter.ResetSession();
            _hasWaitingPause = true;
            _pauser.RequestPause();
        }

        public void Enter()
        {
            if (_isEntered)
            {
                return;
            }

            _isEntered = true;
            _inputReader.MovementKeyPressed += OnMoveStart;
            _musicPlayer.Play(_playlist);
            _inputReader.enabled = true;
        }

        public void Exit()
        {
            _isEntered = false;
            _inputReader.MovementKeyPressed -= OnMoveStart;

            if (_hasWaitingPause)
            {
                _hasWaitingPause = false;
                _pauser.RequestResume();
            }

            while (_pauseRequests > 0)
            {
                ResumeByRequest();
            }
        }

        public void Cleanup()
        {
            Exit();
            _inputReader.enabled = false;
            _activity.SetRunning(false);
            _playerSetup.StopSession();
        }

        public void PauseByRequest()
        {
            _pauseRequests++;
            _pauser.RequestPause();
        }

        public void ResumeByRequest()
        {
            if (_pauseRequests > 0)
            {
                _pauseRequests--;
                _pauser.RequestResume();
            }
        }

        private void OnMoveStart()
        {
            if (_isEntered && _pauseRequests == 0)
            {
                StartRequested?.Invoke();
            }
        }

        private void ReportGenerationProgress(float progress)
        {
            PreparationProgressChanged?.Invoke(progress * 0.85f);
        }

        private void ReportOutlineProgress(float progress)
        {
            PreparationProgressChanged?.Invoke(0.85f + (progress * 0.15f));
        }
    }
}
