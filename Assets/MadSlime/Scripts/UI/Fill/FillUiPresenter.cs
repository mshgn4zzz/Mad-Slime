using Ads;
using Cysharp.Threading.Tasks;
using Economy;
using Game;
using Saves;
using ShapeFill;
using System;
using System.Threading;
using UI.Spawning;
using UI.Windows;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
namespace UI.Fill
{
    public class FillUiPresenter : MonoBehaviour
    {
        public enum WindowAction
        {
            None,
            NextLevel,
            Restart,
            Menu,
            Rescue
        }

        [SerializeField] private Button _pauseButton;
        [SerializeField] private PauseMenu _pauseMenuPrefab;
        [SerializeField] private WinMenu _winMenuPrefab;
        [SerializeField] private FailMenu _failMenuPrefab;
        [SerializeField] private YandexConfig _yandexConfig;

        private UiSpawner _uiSpawner;
        private FillSceneOrchestrator _orchestrator;
        private Wallet _wallet;
        private AdScheduler _adScheduler;
        private ISaveConfirmation _saveConfirmation;
        private WinMenu _activeWinMenu;
        private FailMenu _activeFailMenu;
        private FillPauseFlow _pauseFlow;
        private FillRescueFlow _rescueFlow;
        private CancellationTokenSource _windowCancellation;
        private WindowAction _pendingAction;
        private bool _isActive;
        private bool _isClosingWindow;
        private bool _isDoubleRewardRequested;
        private bool _isDoubleRewardGranted;
        private int _lastRewardAmount;
        private int _lastFailRewardAmount;

        [Inject]
        public void Construct(UiSpawner uiSpawner, FillSceneOrchestrator orchestrator, Wallet wallet,
            AdScheduler adScheduler, ISaveConfirmation saveConfirmation)
        {
            _uiSpawner = uiSpawner;
            _orchestrator = orchestrator;
            _wallet = wallet;
            _adScheduler = adScheduler;
            _saveConfirmation = saveConfirmation;
            _pauseFlow = new FillPauseFlow(uiSpawner, _pauseMenuPrefab, orchestrator,
                CanOpenPauseMenu, CompleteWindowActionRequest, OnPauseCloseCompleted);
            _rescueFlow = new FillRescueFlow(adScheduler, _yandexConfig, CanResolveRescue, OnRescueResolved);
        }

        private bool CanOpenPauseMenu()
        {
            return _isActive && _activeWinMenu == null && _activeFailMenu == null && _isClosingWindow == false;
        }

        private void CompleteWindowActionRequest(WindowAction action)
        {
            _isClosingWindow = true;
            _pendingAction = action;
        }

        private void OnPauseCloseCompleted()
        {
            _isClosingWindow = false;
            CompleteWindowAction();
        }

        private bool CanResolveRescue()
        {
            return _isActive && _isClosingWindow == false && _activeFailMenu != null;
        }

        private void OnRescueResolved()
        {
            CloseFailMenu(WindowAction.Rescue);
        }

        private void Awake()
        {
            if (_uiSpawner == null)
            {
                throw new InvalidOperationException(
                    $"{name}: UiSpawner was not injected. Check that FillLifetimeScope "
                        + $"registers UiSpawner and FillUiPresenter.");
            }

            if (_orchestrator == null)
            {
                throw new InvalidOperationException(
                    $"{name}: FillSceneOrchestrator was not injected. Check that FillLifetimeScope "
                        + $"registers FillSceneOrchestrator and FillUiPresenter.");
            }

            if (_wallet == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Wallet was not injected. Check that FillLifetimeScope registers "
                        + $"Wallet and FillUiPresenter.");
            }

            if (_adScheduler == null)
            {
                throw new InvalidOperationException(
                    $"{name}: AdScheduler was not injected. Check that FillLifetimeScope "
                        + $"registers AdScheduler and FillUiPresenter.");
            }

            if (_pauseButton == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PauseButton is not assigned. Drag a Button into the _pauseButton field.");
            }

            if (_pauseMenuPrefab == null || _winMenuPrefab == null || _failMenuPrefab == null)
            {
                throw new InvalidOperationException(
                    $"{name}: a window prefab is not assigned. Drag the PauseMenu, WinMenu and "
                        + $"FailMenu prefabs into the fields.");
            }

            if (_yandexConfig == null)
            {
                throw new InvalidOperationException(
                    $"{name}: YandexConfig is not assigned. Drag the YandexConfig asset into the _yandexConfig field.");
            }
        }

        private void OnEnable()
        {
            if (_orchestrator.IsFinished == true)
            {
                return;
            }

            _isActive = true;
            _windowCancellation = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            _orchestrator.Failed += OnGameFailed;
            _orchestrator.Win += OnGameWin;
            _orchestrator.Finished += OnSessionFinished;
            _pauseButton.onClick.AddListener(OnPauseButtonClick);
        }

        private void OnDisable()
        {
            _orchestrator.Failed -= OnGameFailed;
            _orchestrator.Win -= OnGameWin;
            _orchestrator.Finished -= OnSessionFinished;
            _pauseButton.onClick.RemoveListener(OnPauseButtonClick);
            OnSessionFinished();
        }

        private void OnSessionFinished()
        {
            _isActive = false;
            _isDoubleRewardRequested = false;
            _pendingAction = WindowAction.None;

            if (_windowCancellation != null)
            {
                _windowCancellation.Cancel();
                _windowCancellation.Dispose();
                _windowCancellation = null;
            }

            ReleaseWinMenu();
            ReleaseFailMenu();
            _pauseFlow.Release();
        }

        private void OnGameWin(int rewardAmount)
        {
            if (_isActive == false || _activeWinMenu != null)
            {
                return;
            }

            _lastRewardAmount = rewardAmount;
            _isDoubleRewardRequested = false;
            _isDoubleRewardGranted = false;
            _isClosingWindow = false;
            _activeWinMenu = _uiSpawner.Spawn(_winMenuPrefab, UiLayer.Popup);
            _activeWinMenu.Initialize(rewardAmount, _lastFailRewardAmount, true);
            _activeWinMenu.NextLevelRequested += OnWinNextLevelRequested;
            _activeWinMenu.DoubleRewardRequested += OnWinDoubleRewardRequested;
            _activeWinMenu.MenuRequested += OnWinMenuRequested;
            _activeWinMenu.Closed += OnWinMenuClosed;
            _uiSpawner.Show(_activeWinMenu);
            _activeWinMenu.PlayRewardCountUp(rewardAmount);
        }

        private void OnGameFailed(int rewardAmount)
        {
            if (_isActive == false || _activeFailMenu != null)
            {
                return;
            }

            _lastFailRewardAmount = rewardAmount;
            _rescueFlow.Reset();
            _isClosingWindow = false;
            _activeFailMenu = _uiSpawner.Spawn(_failMenuPrefab, UiLayer.Popup);
            _activeFailMenu.Initialize(rewardAmount, _orchestrator.CanRescueFill);
            _activeFailMenu.RescueRequested += OnFailRescueRequested;
            _activeFailMenu.RestartRequested += OnFailRestartRequested;
            _activeFailMenu.MenuRequested += OnFailMenuRequested;
            _activeFailMenu.Closed += OnFailMenuClosed;
            _uiSpawner.Show(_activeFailMenu);
        }

        private void OnFailRescueRequested()
        {
            if (_isActive == false || _isClosingWindow == true || _activeFailMenu == null)
            {
                return;
            }

            _activeFailMenu.LockRescue();
            _rescueFlow.Begin();
        }

        private void CloseFailMenu(WindowAction action)
        {
            if (_isActive == false || _activeFailMenu == null || _isClosingWindow == true)
            {
                return;
            }

            _isClosingWindow = true;
            _rescueFlow.StopRequest();
            _pendingAction = action;
            UnsubscribeFailRequests();
            _activeFailMenu.BeginClose();
        }

        private void OnFailMenuClosed()
        {
            ReleaseFailMenu();
            CompleteWindowAction();
        }

        private void OnFailRestartRequested()
        {
            CloseFailMenu(WindowAction.Restart);
        }

        private void OnFailMenuRequested()
        {
            CloseFailMenu(WindowAction.Menu);
        }

        private void CloseWinMenu(WindowAction action)
        {
            if (_isActive == false || _activeWinMenu == null || _isClosingWindow == true)
            {
                return;
            }

            _isClosingWindow = true;
            _isDoubleRewardRequested = false;
            _pendingAction = action;
            UnsubscribeWinRequests();
            _activeWinMenu.BeginClose();
        }

        private void OnWinMenuClosed()
        {
            ReleaseWinMenu();
            CompleteWindowAction();
        }

        private void OnWinNextLevelRequested()
        {
            CloseWinMenu(WindowAction.NextLevel);
        }

        private void OnWinMenuRequested()
        {
            CloseWinMenu(WindowAction.Menu);
        }

        private void OnWinDoubleRewardRequested()
        {
            if (_isActive == false || _isClosingWindow == true || _activeWinMenu == null
                || _isDoubleRewardRequested == true || _isDoubleRewardGranted == true)
            {
                return;
            }

            _isDoubleRewardRequested = true;
            _activeWinMenu.SetDoubleRewardAvailable(false);
            _adScheduler.ShowDoubleReward(OnDoubleRewardGranted, OnDoubleRewardNotGranted);
        }

        private void OnDoubleRewardNotGranted()
        {
            if (_isActive == false || _isDoubleRewardRequested == false)
            {
                return;
            }

            _isDoubleRewardRequested = false;

            if (_isClosingWindow == false && _activeWinMenu != null)
            {
                _activeWinMenu.SetDoubleRewardAvailable(true);
            }
        }

        private void OnDoubleRewardGranted()
        {
            if (_isActive == false || _isClosingWindow == true || _activeWinMenu == null
                || _isDoubleRewardGranted == true || _isDoubleRewardRequested == false)
            {
                return;
            }

            _isDoubleRewardGranted = true;
            _isDoubleRewardRequested = false;
            _wallet.Add(_lastRewardAmount);
            ShowDoubleRewardAsync(_windowCancellation.Token).Forget();
        }

        private async UniTaskVoid ShowDoubleRewardAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _saveConfirmation.ConfirmSavedAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (_isActive == true && _isClosingWindow == false && _activeWinMenu != null)
            {
                _activeWinMenu.PlayRewardCountUp(checked(_lastRewardAmount * 2));
            }
        }

        private void OnPauseButtonClick()
        {
            if (_isClosingWindow == true)
            {
                return;
            }

            _pauseFlow.Open();
        }

        private void CompleteWindowAction()
        {
            WindowAction action = _pendingAction;
            _pendingAction = WindowAction.None;
            _isClosingWindow = false;

            if (_isActive == false)
            {
                return;
            }

            switch (action)
            {
                case WindowAction.NextLevel:
                case WindowAction.Restart:
                    _orchestrator.ReloadLevel();
                    break;
                case WindowAction.Menu:
                    _orchestrator.ExitToMenu();
                    break;
                case WindowAction.Rescue:
                    _orchestrator.RescueFill();
                    break;
            }
        }

        private void UnsubscribeWinRequests()
        {
            _activeWinMenu.NextLevelRequested -= OnWinNextLevelRequested;
            _activeWinMenu.DoubleRewardRequested -= OnWinDoubleRewardRequested;
            _activeWinMenu.MenuRequested -= OnWinMenuRequested;
        }

        private void UnsubscribeFailRequests()
        {
            _activeFailMenu.RescueRequested -= OnFailRescueRequested;
            _activeFailMenu.RestartRequested -= OnFailRestartRequested;
            _activeFailMenu.MenuRequested -= OnFailMenuRequested;
        }

        private void ReleaseWinMenu()
        {
            if (_activeWinMenu == null)
            {
                return;
            }

            UnsubscribeWinRequests();
            _activeWinMenu.Closed -= OnWinMenuClosed;
            _activeWinMenu.BeginClose();
            _uiSpawner.Release(_activeWinMenu);
            _activeWinMenu = null;
        }

        private void ReleaseFailMenu()
        {
            if (_activeFailMenu == null)
            {
                return;
            }

            UnsubscribeFailRequests();
            _activeFailMenu.Closed -= OnFailMenuClosed;
            _activeFailMenu.BeginClose();
            _uiSpawner.Release(_activeFailMenu);
            _activeFailMenu = null;
        }
    }
}
