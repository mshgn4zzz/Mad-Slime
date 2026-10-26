using Audio;
using Cysharp.Threading.Tasks;
using Economy;
using Game;
using Saves;
using System;
using System.Threading;
using UnityEngine;
using VContainer;
namespace ShapeFill
{
    public class FillSceneOrchestrator : MonoBehaviour
    {
        [SerializeField] private Playlist _musicPlaylist;
        [SerializeField] private MeshRenderer _floorRenderer;

        [Tooltip("Пауза перед показом окна победы после финала заливки: игрок видит конфетти и пунш формы (с).")]
        [SerializeField, Min(0f)] private float _winWindowDelay = 1f;

        private FillConfig _config;
        private IMusicPlayer _musicPlayer;
        private LevelThemeApplier _themeApplier;
        private ShapeFillOrchestrator _fillOrchestrator;
        private GridCameraFitter _cameraFitter;
        private SceneNavigator _sceneNavigator;
        private FillExitFlow _exitFlow;
        private Rewarder _rewarder;
        private Pauser _pauser;
        private IGameplayReporter _gameplayReporter;
        private bool _isGameplayStarted;
        private bool _isPreparing;
        private bool _hasStarted;
        private bool _isFinished;
        private bool _isCompletionHandled;
        private bool _isRescueCommitted;
        private bool _canRescue;
        private FillResult _result;
        private FillSessionSaver _saver;
        private CancellationTokenSource _sessionCancellation;

        public event Action<int> Win;
        public event Action<int> Failed;

        public event Action Finished;

        public bool CanRescueFill => _canRescue;

        public bool IsFinished => _isFinished;

        [Inject]
        public void Construct(LevelThemeApplier themeApplier, IMusicPlayer musicPlayer,
            ShapeFillOrchestrator fillOrchestrator, GridCameraFitter cameraFitter, SceneNavigator sceneNavigator,
            FillExitFlow exitFlow, Rewarder rewarder, Pauser pauser, IGameplayReporter gameplayReporter,
            FillConfig config, FillSessionSaver saver)
        {
            _themeApplier = themeApplier;
            _config = config;
            _musicPlayer = musicPlayer;
            _fillOrchestrator = fillOrchestrator;
            _cameraFitter = cameraFitter;
            _sceneNavigator = sceneNavigator;
            _exitFlow = exitFlow;
            _rewarder = rewarder;
            _pauser = pauser;
            _gameplayReporter = gameplayReporter;
            _saver = saver;
        }

        private void Awake()
        {
            if (_floorRenderer == null)
            {
                throw new InvalidOperationException(
                    $"{name}: FloorRenderer is not assigned. Drag the floor MeshRenderer into "
                        + $"the _floorRenderer field.");
            }

            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: FillConfig was not injected. Check that FillLifetimeScope has the FillConfig assigned.");
            }

            if (_musicPlaylist == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Music playlist is not assigned. Drag a Playlist asset into the _musicPlaylist field.");
            }

            if (_themeApplier == null)
            {
                throw new InvalidOperationException(
                    $"{name}: LevelThemeApplier was not injected. Check that FillLifetimeScope "
                        + $"registers LevelThemeApplier and FillSceneOrchestrator.");
            }

            if (_exitFlow == null)
            {
                throw new InvalidOperationException(
                    $"{name}: FillExitFlow was not injected. Check that FillLifetimeScope "
                        + $"registers FillExitFlow and FillSceneOrchestrator.");
            }

            if (_musicPlayer == null)
            {
                throw new InvalidOperationException(
                    $"{name}: IMusicPlayer was not injected. Check that ProjectLifetimeScope "
                        + $"registers the audio players.");
            }

            if (_rewarder == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Rewarder was not injected. Check that FillLifetimeScope "
                        + $"registers Rewarder and FillSceneOrchestrator.");
            }

            if (_pauser == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Pauser was not injected. Check that FillLifetimeScope "
                        + $"registers Pauser and FillSceneOrchestrator.");
            }

            if (_saver == null)
            {
                throw new InvalidOperationException(
                    $"{name}: FillSessionSaver was not injected. Check that FillLifetimeScope "
                        + $"registers FillSessionSaver and FillSceneOrchestrator.");
            }

            if (_fillOrchestrator == null)
            {
                throw new InvalidOperationException(
                    $"{name}: ShapeFillOrchestrator was not injected. Check that FillLifetimeScope "
                        + $"registers ShapeFillOrchestrator and FillSceneOrchestrator.");
            }

            if (_gameplayReporter == null)
            {
                throw new InvalidOperationException(
                    $"{name}: IGameplayReporter was not injected. Check that "
                        + $"ProjectLifetimeScope registers the YG2 gameplay adapter.");
            }

            if (_sceneNavigator == null)
            {
                throw new InvalidOperationException(
                    $"{name}: SceneNavigator was not injected. Check that ProjectLifetimeScope "
                        + $"registers SceneNavigator and FillLifetimeScope registers FillSceneOrchestrator.");
            }

            if (_cameraFitter == null)
            {
                throw new InvalidOperationException(
                    $"{name}: GridCameraFitter was not injected. Check that FillLifetimeScope "
                        + $"registers GridCameraFitter and FillSceneOrchestrator.");
            }
        }

        private void OnEnable()
        {
            if (_isFinished == true)
            {
                return;
            }

            _sessionCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy());
            _fillOrchestrator.FillCompleted += OnFillCompleted;
        }

        private void OnDisable()
        {
            if (_sceneNavigator != null)
            {
                _sceneNavigator.UnregisterSession(StartSession);
            }

            if (_hasStarted == true && _fillOrchestrator != null)
            {
                Finish();
            }

            if (_fillOrchestrator != null)
            {
                _fillOrchestrator.FillCompleted -= OnFillCompleted;
            }
        }

        private void Start()
        {
            _sceneNavigator.RegisterSession(PrepareSessionAsync, StartSession, Finish, _sessionCancellation.Token);
        }

        private async UniTask PrepareSessionAsync(CancellationToken cancellationToken)
        {
            if (_isFinished == true || _isPreparing == true || _hasStarted == true)
            {
                throw new InvalidOperationException("Fill session cannot be prepared twice.");
            }

            _isPreparing = true;

            try
            {
                _themeApplier.Apply(_floorRenderer, _fillOrchestrator);
                _fillOrchestrator.Prepare(_floorRenderer.bounds);
                _result = _fillOrchestrator.CalculateResult();
                _fillOrchestrator.FitCamera(_cameraFitter);
                await _saver.CommitFillAndSave(_result, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch
            {
                _isPreparing = false;
                throw;
            }
        }

        private void StartSession()
        {
            if (_hasStarted == true || _isFinished == true)
            {
                return;
            }

            _hasStarted = true;
            _isPreparing = false;
            _musicPlayer.Play(_musicPlaylist);
            _fillOrchestrator.Show(_result);
            StartGameplay();
        }

        public void ReloadLevel()
        {
            ExitTo(SceneId.Game);
        }

        public void ExitToMenu()
        {
            ExitTo(SceneId.Menu);
        }

        private void ExitTo(SceneId targetSceneId)
        {
            if (_isFinished == true)
            {
                return;
            }

            Finish();
            _exitFlow.Exit(targetSceneId);
        }

        public void RescueFill()
        {
            if (_isFinished == true || _canRescue == false || _isPreparing == true || _isRescueCommitted == true)
            {
                return;
            }

            _isPreparing = true;
            _canRescue = false;
            _isRescueCommitted = true;
            RescueFillAsync().Forget();
        }

        private async UniTaskVoid RescueFillAsync()
        {
            CancellationToken cancellationToken = _sessionCancellation.Token;

            try
            {
                await _saver.CommitRescueAndSave(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                _isCompletionHandled = false;
                _isPreparing = false;
                _fillOrchestrator.RescueShow();
                StartGameplay();
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        public void PauseByRequest()
        {
            if (_isFinished == false)
            {
                _pauser.RequestPause();
            }
        }

        public void ResumeByRequest()
        {
            _pauser.RequestResume();
        }

        private void StartGameplay()
        {
            if (_isGameplayStarted == true || _isFinished == true || _isCompletionHandled == true)
            {
                return;
            }

            _isGameplayStarted = true;
            _gameplayReporter.ReportStart();
        }

        private void StopGameplay()
        {
            if (_isGameplayStarted == false)
            {
                return;
            }

            _isGameplayStarted = false;
            _gameplayReporter.ReportStop();
        }

        private void OnFillCompleted(float percent)
        {
            if (_isFinished == true || _isPreparing == true || _isCompletionHandled == true)
            {
                return;
            }

            _isCompletionHandled = true;
            StopGameplay();

            if (_rewarder.IsWin == true)
            {
                ShowWinWindowDelayed().Forget();
                return;
            }

            _canRescue = _fillOrchestrator.CanRescue();
            Failed?.Invoke(_rewarder.GrantedTotal);
        }

        private async UniTaskVoid ShowWinWindowDelayed()
        {
            try
            {
                if (_winWindowDelay > 0f)
                {
                    await UniTask.Delay((int)(_winWindowDelay * 1000f),
                        cancellationToken: _sessionCancellation.Token);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (_isFinished == false)
            {
                Win?.Invoke(_rewarder.GrantedTotal);
            }
        }

        private void Finish()
        {
            if (_isFinished == true)
            {
                return;
            }

            _isFinished = true;
            _canRescue = false;
            StopGameplay();
            _fillOrchestrator.Stop();

            if (_sessionCancellation != null)
            {
                _sessionCancellation.Cancel();
                _sessionCancellation.Dispose();
                _sessionCancellation = null;
            }

            Finished?.Invoke();
        }

    }
}
