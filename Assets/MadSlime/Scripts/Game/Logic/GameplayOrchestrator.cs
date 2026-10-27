using Audio;
using Collectables;
using Cysharp.Threading.Tasks;
using Levels;
using PlayerInput;
using Quota;
using Saves;
using System;
using System.Threading;
using UnityEngine;
using VContainer;
namespace Game
{
    public class GameplayOrchestrator : MonoBehaviour
    {
        private const float LevelGenerationProgressWeight = 0.85f;
        private const float OutlinePreparationProgressWeight = 0.15f;

        private enum SessionState
        {
            Preparing,
            WaitingForStart,
            Running,
            Finished
        }

        [SerializeField] private Playlist _musicPlaylist;

        [SerializeField, Min(0f)] private float _endDelay = 1f;

        [SerializeField, Min(0.1f)] private float _finalSeconds = 20f;

        private LevelConfigResolver _configResolver;
        private LevelGenerator _levelGenerator;
        private ItemCollector _itemCollector;
        private IMusicPlayer _musicPlayer;
        private ILevelStorage _levelStorage;
        private QuotaCounter _quotaCounter;
        private SceneNavigator _sceneNavigator;
        private Timer _timer;
        private PlayerInputReader _inputReader;
        private Pauser _pauser;
        private IGameplayReporter _gameplayReporter;
        private GameplayActivityController _activity;
        private FinalCountdownEffects _finalCountdownEffects;
        private GameplayPlayerSetup _playerSetup;
        private CancellationTokenSource _sessionSource;
        private CancellationTokenSource _finishDelaySource;
        private SessionState _state = SessionState.Preparing;
        private bool _isFinalCountdownStarted;

        public event Action SceneSessionPrepared;

        public event Action SceneSessionStarted;

        public event Action GameplayStarted;

        public event Action SessionFinished;

        [Inject]
        public void Construct(LevelConfigResolver configResolver, ILevelStorage levelStorage, QuotaCounter quotaCounter,
            LevelGenerator levelGenerator, ItemCollector itemCollector, IMusicPlayer musicPlayer,
            SceneNavigator sceneNavigator, Timer timer, PlayerInputReader inputReader,
            Pauser pauser, IGameplayReporter gameplayReporter, GameplayPlayerSetup playerSetup,
            GameplayActivityController activity, FinalCountdownEffects finalCountdownEffects)
        {
            _configResolver = configResolver;
            _levelGenerator = levelGenerator;
            _itemCollector = itemCollector;
            _levelStorage = levelStorage;
            _quotaCounter = quotaCounter;
            _musicPlayer = musicPlayer;
            _sceneNavigator = sceneNavigator;
            _timer = timer;
            _inputReader = inputReader;
            _pauser = pauser;
            _gameplayReporter = gameplayReporter;
            _playerSetup = playerSetup;
            _activity = activity;
            _finalCountdownEffects = finalCountdownEffects;
        }

        private void Awake()
        {
            if (_configResolver == null)
            {
                throw new InvalidOperationException(
                    $"{name}: dependencies were not injected. GameLifetimeScope must be the "
                        + $"first object in the scene hierarchy.");
            }

            if (_musicPlaylist == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Music playlist is not assigned. Drag a Playlist asset into the _musicPlaylist field.");
            }

            if (_levelGenerator == null)
            {
                throw new InvalidOperationException(
                    $"{name}: LevelGenerator was not injected. Check that GameLifetimeScope "
                        + $"registers LevelGenerator and GameplayOrchestrator.");
            }

            if (_itemCollector == null)
            {
                throw new InvalidOperationException(
                    $"{name}: ItemCollector was not injected. Check that GameLifetimeScope registers "
                        + $"ItemCollector and GameplayOrchestrator.");
            }

            if (_sceneNavigator == null)
            {
                throw new InvalidOperationException(
                    $"{name}: SceneNavigator was not injected. Check that ProjectLifetimeScope "
                        + $"registers SceneNavigator.");
            }

            if (_gameplayReporter == null)
            {
                throw new InvalidOperationException(
                    $"{name}: IGameplayReporter was not injected. Check that "
                        + $"ProjectLifetimeScope registers the YG2 gameplay adapter.");
            }

            if (_quotaCounter == null)
            {
                throw new InvalidOperationException(
                    $"{name}: QuotaCounter was not injected. Check that ProjectLifetimeScope "
                        + $"registers QuotaCounter and GameplayOrchestrator.");
            }

            if (_activity == null)
            {
                throw new InvalidOperationException(
                    $"{name}: GameplayActivityController was not injected. Check that GameLifetimeScope "
                        + $"registers GameplayActivityController and GameplayOrchestrator.");
            }

            if (_finalCountdownEffects == null)
            {
                throw new InvalidOperationException(
                    $"{name}: FinalCountdownEffects was not injected. Check that GameLifetimeScope "
                        + $"registers FinalCountdownEffects and GameplayOrchestrator.");
            }

            if (_playerSetup == null)
            {
                throw new InvalidOperationException(
                    $"{name}: GameplayPlayerSetup was not injected. Check that GameLifetimeScope "
                        + $"registers GameplayPlayerSetup and GameplayOrchestrator.");
            }

            if (_timer == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Timer was not injected. Check that GameLifetimeScope "
                        + $"registers Timer and GameplayOrchestrator.");
            }

            if (_inputReader == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerInputReader was not injected. Check that GameLifetimeScope "
                        + $"registers PlayerInputReader and GameplayOrchestrator.");
            }

            if (_pauser == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Pauser was not injected. Check that GameLifetimeScope "
                        + $"registers Pauser and GameplayOrchestrator.");
            }
        }

        private void OnEnable()
        {
            _sessionSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            _inputReader.MovementKeyPressed += OnMovementKeyPressed;
            _timer.Ticked += OnTimerTicked;
            _timer.Finished += OnSessionCompleted;
            _pauser.StateChanged += OnPauseStateChanged;
            _quotaCounter.QuotaCompleted += OnSessionCompleted;
        }

        private void OnDisable()
        {
            FinishSession();
            _sceneNavigator.UnregisterSession(RunSession);
            _sessionSource.Dispose();
            _sessionSource = null;
            _inputReader.MovementKeyPressed -= OnMovementKeyPressed;
            _timer.Ticked -= OnTimerTicked;
            _timer.Finished -= OnSessionCompleted;
            _pauser.StateChanged -= OnPauseStateChanged;

            _quotaCounter.QuotaCompleted -= OnSessionCompleted;
            _quotaCounter.Detach();
        }

        public void ExitToMenu()
        {
            FinishSession();
            NavigateTo(SceneId.Menu).Forget();
        }

        public void PauseByRequest()
        {
            _pauser.RequestPause();
        }

        public void ResumeByRequest()
        {
            _pauser.RequestResume();
        }

        private void Start()
        {
            _sceneNavigator.RegisterSession(PrepareSessionAsync, RunSession, FinishSession, _sessionSource.Token);
        }

        private async UniTask PrepareSessionAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _inputReader.enabled = false;
            _playerSetup.PrepareSession();
            _finalCountdownEffects.Stop();

            LevelConfig config = _configResolver.GetConfigFor(_levelStorage.CurrentLevel);

            _timer.Setup(config.TimerDuration);
            await _levelGenerator.GenerateAsync(
                ReportLevelGenerationProgress, cancellationToken);
            await _playerSetup.PrepareOutlineAsync(_levelGenerator.SpawnedItems,
                ReportOutlinePreparationProgress, cancellationToken);
            _quotaCounter.ResetSession();
            _isFinalCountdownStarted = false;
            _pauser.RequestPause();
            OnPauseStateChanged();
            SceneSessionPrepared?.Invoke();
        }

        private void ReportLevelGenerationProgress(float progress)
        {
            _sceneNavigator.ReportPreparationProgress(progress * LevelGenerationProgressWeight);
        }

        private void ReportOutlinePreparationProgress(float progress)
        {
            float totalProgress = LevelGenerationProgressWeight + progress * OutlinePreparationProgressWeight;
            _sceneNavigator.ReportPreparationProgress(totalProgress);
        }

        private void RunSession()
        {
            if (TryTransitTo(SessionState.WaitingForStart) == false)
            {
                return;
            }

            _musicPlayer.Play(_musicPlaylist);
            _quotaCounter.Attach(_itemCollector);
            OnPauseStateChanged();
            _inputReader.enabled = true;

            Action sceneSessionStarted = SceneSessionStarted;
            sceneSessionStarted?.Invoke();
        }

        private void OnPauseStateChanged()
        {
            bool isRunning = _state == SessionState.Running && _pauser.IsPaused() == false;
            _activity.SetRunning(isRunning);
        }

        private void FinishSession()
        {
            bool wasRunning = _state == SessionState.Running;

            if (TryTransitTo(SessionState.Finished) == false)
            {
                return;
            }

            _sessionSource.Cancel();

            if (_finishDelaySource != null)
            {
                _finishDelaySource.Cancel();
                _finishDelaySource.Dispose();
                _finishDelaySource = null;
            }

            TeardownSession(wasRunning);
        }

        private void TeardownSession(bool wasRunning)
        {
            _playerSetup.StopSession();
            _timer.Stop();
            _itemCollector.StopCollecting();
            _quotaCounter.Detach();
            OnPauseStateChanged();
            _finalCountdownEffects.Stop();

            if (wasRunning)
            {
                _gameplayReporter.ReportStop();
            }

            SessionFinished?.Invoke();
        }

        private void OnMovementKeyPressed()
        {
            if (_state != SessionState.WaitingForStart || TryTransitTo(SessionState.Running) == false)
            {
                return;
            }

            Action gameplayStarted = GameplayStarted;
            gameplayStarted?.Invoke();

            _pauser.RequestResume();
            _timer.StartCount();
            _gameplayReporter.ReportStart();
        }

        private void OnTimerTicked(float remainingSeconds)
        {
            if (_state != SessionState.Running)
            {
                return;
            }

            float urgency = 0f;

            if (_finalSeconds > 0f)
            {
                urgency = 1f - Mathf.Clamp01(remainingSeconds / _finalSeconds);
            }

            _finalCountdownEffects.SetUrgency(urgency);

            if (_isFinalCountdownStarted == true)
            {
                return;
            }

            if (remainingSeconds > _finalSeconds)
            {
                return;
            }

            _isFinalCountdownStarted = true;
            _finalCountdownEffects.Start();
        }

        private void OnSessionCompleted()
        {
            if (TryTransitTo(SessionState.Finished) == false)
            {
                return;
            }

            FinishGame();
        }

        private bool TryTransitTo(SessionState targetState)
        {
            if (_state == SessionState.Finished)
            {
                return false;
            }

            if (targetState <= _state)
            {
                return false;
            }

            _state = targetState;
            return true;
        }

        private void FinishGame()
        {
            _pauser.RequestPause();
            TeardownSession(true);
            _finishDelaySource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            FinishDelayAsync(_finishDelaySource.Token).Forget();
        }

        private async UniTaskVoid FinishDelayAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_endDelay > 0f)
                {
                    await UniTask.Delay
                    (
                        (int)(_endDelay * 1000f),
                        DelayType.Realtime,
                        cancellationToken: cancellationToken
                    );
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            NavigateTo(SceneId.Fill).Forget();
        }

        private async UniTaskVoid NavigateTo(SceneId targetSceneId)
        {
            if (_sceneNavigator.IsTransitioning == true)
            {
                return;
            }

            _pauser.ResetToPlay();

            if (targetSceneId == SceneId.Game)
            {
                await _sceneNavigator.LoadGameAsync();
                return;
            }

            if (targetSceneId == SceneId.Fill)
            {
                await _sceneNavigator.LoadFillAsync();
                return;
            }

            await _sceneNavigator.LoadMenuAsync();
        }
    }
}
