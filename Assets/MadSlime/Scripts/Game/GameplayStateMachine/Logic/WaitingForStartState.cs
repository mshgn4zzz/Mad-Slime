using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.Game.GameplayStateMachine.Interfaces;
using MadSlime.Game.Logic;
using MadSlime.Levels.Data;
using MadSlime.Levels.Logic;
using MadSlime.PlayerInput.Logic;
using MadSlime.Quota.Logic;
using MadSlime.Saves.Interfaces;
using Timer = MadSlime.Game.Logic.Timer;

namespace MadSlime.Game.GameplayStateMachine.Logic
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
        private bool _isWaitingPaused;
        private PauseRequests _pauseRequests;

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
            _pauseRequests = new PauseRequests(pauser);
            _musicPlayer = musicPlayer;
        }

        public event Action StartRequested;

        public event Action<float> PreparationProgressChanged;

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
            await _playerSetup.PrepareOutlineAsync(
                _levelGenerator.SpawnedItems,
                ReportOutlineProgress,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            _quotaCounter.ResetSession();
            _isWaitingPaused = true;
            _pauser.IncreasePauseRequests();
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

            if (_isWaitingPaused)
            {
                _isWaitingPaused = false;
                _pauser.DecreasePauseRequests();
            }

            _pauseRequests.ReleaseAll();
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
            _pauseRequests.Acquire();
        }

        public void ResumeByRequest()
        {
            _pauseRequests.Release();
        }

        private void ReportGenerationProgress(float progress)
        {
            PreparationProgressChanged?.Invoke(progress * 0.85f);
        }

        private void ReportOutlineProgress(float progress)
        {
            PreparationProgressChanged?.Invoke(0.85f + (progress * 0.15f));
        }

        private void OnMoveStart()
        {
            if (_isEntered && _pauseRequests.Count == 0)
            {
                StartRequested?.Invoke();
            }
        }
    }
}
