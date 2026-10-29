using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Ads.Logic;
using MadSlime.Game.Data;
using MadSlime.GameFSM.Scenes;
using MadSlime.ShapeFill.Input;
using MadSlime.UI.Fill.StateMachine;
using MadSlime.UI.Spawning;
using MadSlime.UI.Windows;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MadSlime.UI.Fill
{
    public class FillUiPresenter : MonoBehaviour
    {
        private readonly FillResultView _resultView = new FillResultView();
        [SerializeField]
        private Button _pauseButton;
        [SerializeField]
        private PauseMenu _pauseMenuPrefab;
        [SerializeField]
        private WinMenu _winMenuPrefab;
        [SerializeField]
        private FailMenu _failMenuPrefab;
        [SerializeField]
        private YandexConfig _yandexConfig;
        [SerializeField, Min(0f)]
        private float _resultWindowDelay = 1f;

        private UiSpawner _uiSpawner;
        private FillSceneState _orchestrator;
        private AdScheduler _adScheduler;
        private FillTapButton _tapButton;
        private FillUiStateMachine _machine;
        private CancellationTokenSource _windowCancellation;
        private WinMenuSubscription _winSubscription;
        private FailMenuSubscription _failSubscription;
        private PauseMenuSubscription _pauseSubscription;
        private WinMenu _activeWinMenu;
        private FailMenu _activeFailMenu;
        private PauseMenu _activePauseMenu;

        private void Awake()
        {
            if (_machine == null || _pauseButton == null || _pauseMenuPrefab == null || _winMenuPrefab == null
                || _failMenuPrefab == null
                || _yandexConfig == null)
            {
                throw new InvalidOperationException($"{name}: Fill UI references are incomplete.");
            }
        }

        private void OnEnable()
        {
            _orchestrator.Succeeded += OnGameWin;
            _orchestrator.Failed += OnGameFailed;
            _orchestrator.Finished += OnSessionFinished;
            _orchestrator.PreparationFailed += OnPreparationFailed;
            _tapButton.Pressed += OnFillPressed;
            _pauseButton.onClick.AddListener(OnPauseButtonClick);
            _machine.ResultPresentationRequested += OnResultRequested;
            _machine.PauseRequested += OnPauseRequested;
            _machine.WindowCloseRequested += OnWindowCloseRequested;
            _machine.DoubleRewardAdRequested += OnDoubleRewardAdRequested;
            _machine.RescueAdRequested += OnRescueAdRequested;
            _machine.RewardChanged += OnRewardChanged;
            _machine.NavigationRequested += OnNavigationRequested;
            _machine.InteractionChanged += OnInteractionChanged;
            _machine.AccelerateRequested += OnAccelerateRequested;
            _machine.OperationFailed += OnOperationFailed;
            _windowCancellation = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            if (_orchestrator.IsFinished == false)
            {
                _machine.Start();
            }
        }

        private void OnDisable()
        {
            _orchestrator.Succeeded -= OnGameWin;
            _orchestrator.Failed -= OnGameFailed;
            _orchestrator.Finished -= OnSessionFinished;
            _orchestrator.PreparationFailed -= OnPreparationFailed;
            _tapButton.Pressed -= OnFillPressed;
            _pauseButton.onClick.RemoveListener(OnPauseButtonClick);
            _machine.ResultPresentationRequested -= OnResultRequested;
            _machine.PauseRequested -= OnPauseRequested;
            _machine.WindowCloseRequested -= OnWindowCloseRequested;
            _machine.DoubleRewardAdRequested -= OnDoubleRewardAdRequested;
            _machine.RescueAdRequested -= OnRescueAdRequested;
            _machine.RewardChanged -= OnRewardChanged;
            _machine.NavigationRequested -= OnNavigationRequested;
            _machine.InteractionChanged -= OnInteractionChanged;
            _machine.AccelerateRequested -= OnAccelerateRequested;
            _machine.OperationFailed -= OnOperationFailed;
            OnSessionFinished();
        }

        private void OnDestroy()
        {
            _machine?.Dispose();
            _resultView.Dispose();
        }

        [Inject]
        public void Construct(
            UiSpawner spawner,
            FillSceneState orchestrator,
            AdScheduler ads,
            FillTapButton tapButton,
            FillUiStateMachineFactory factory)
        {
            _uiSpawner = spawner;
            _orchestrator = orchestrator;
            _adScheduler = ads;
            _tapButton = tapButton;
            _machine = factory.Create();
        }

        private async UniTask ShowResultAsync(FillUiResult result, CancellationToken token)
        {
            try
            {
                await _resultView.WaitForResultAsync(_resultWindowDelay, token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            token.ThrowIfCancellationRequested();

            if (result.IsWin)
            {
                _activeWinMenu = _uiSpawner.Spawn(_winMenuPrefab, UiLayer.Popup);
                _activeWinMenu.Initialize(result.Reward, result.PreviousReward, true);
                _winSubscription = new WinMenuSubscription(
                    _activeWinMenu,
                    OnWinNextLevelRequested,
                    OnWinDoubleRewardRequested,
                    OnWinMenuRequested,
                    OnWinMenuClosed);
                _uiSpawner.Show(_activeWinMenu);
                _activeWinMenu.PlayRewardCountUp(result.Reward);
            }
            else
            {
                _activeFailMenu = _uiSpawner.Spawn(_failMenuPrefab, UiLayer.Popup);
                _activeFailMenu.Initialize(result.Reward, result.IsRescueAvailable);
                _failSubscription = new FailMenuSubscription(
                    _activeFailMenu,
                    OnFailRescueRequested,
                    OnFailRestartRequested,
                    OnFailMenuRequested,
                    OnFailMenuClosed);
                _uiSpawner.Show(_activeFailMenu);
            }

            _machine.NotifyResultShown();
        }

        private void ReleaseWinMenu()
        {
            WinMenu menu = _activeWinMenu;

            _activeWinMenu = null;
            _winSubscription?.Dispose();
            _winSubscription = null;

            if (menu == null)
            {
                return;
            }

            _uiSpawner.Release(menu);
        }

        private void ReleaseFailMenu()
        {
            FailMenu menu = _activeFailMenu;

            _activeFailMenu = null;
            _failSubscription?.Dispose();
            _failSubscription = null;

            if (menu == null)
            {
                return;
            }

            _uiSpawner.Release(menu);
        }

        private void ReleasePauseMenu()
        {
            PauseMenu menu = _activePauseMenu;

            _activePauseMenu = null;
            _pauseSubscription?.Dispose();
            _pauseSubscription = null;

            if (menu == null)
            {
                return;
            }

            _uiSpawner.Release(menu);
            _orchestrator.ResumeByRequest();
        }

        private void OnInteractionChanged()
        {
            _pauseButton.interactable = _machine.IsPauseAvailable();

            if (_activeWinMenu != null)
            {
                _activeWinMenu.SetDoubleRewardAvailable(_machine.IsDoubleRewardAvailable());
                _activeWinMenu.SetDoubleRewardRetry(_machine.IsSaveFailed());
            }

            if (_activeFailMenu != null)
            {
                _activeFailMenu.SetRescueAvailable(_machine.IsRescueAvailable());
            }
        }

        private void OnSessionFinished()
        {
            _machine.Stop();
            _resultView.Cancel();

            CancellationTokenSource source = _windowCancellation;

            _windowCancellation = null;
            source?.Cancel();
            source?.Dispose();
            ReleaseWinMenu();
            ReleaseFailMenu();
            ReleasePauseMenu();
        }

        private void OnGameWin(int amount)
        {
            _machine.PresentWin(amount);
        }

        private void OnGameFailed(int amount, bool isRescueAvailable)
        {
            _machine.PresentFail(amount, isRescueAvailable);
        }

        private void OnPreparationFailed(Exception exception)
        {
            _machine.PreparationFailed();
        }

        private void OnFillPressed()
        {
            _machine.RequestAccelerate();
        }

        private void OnAccelerateRequested()
        {
            _orchestrator.Accelerate();
        }

        private void OnPauseButtonClick()
        {
            _machine.RequestPause();
        }

        private void OnResultRequested(FillUiResult result)
        {
            ReleasePauseMenu();
            ReleaseWinMenu();
            ReleaseFailMenu();
            ShowResultAsync(result, _windowCancellation.Token).Forget();
        }

        private void OnPauseRequested()
        {
            _orchestrator.PauseByRequest();
            _activePauseMenu = _uiSpawner.Spawn(_pauseMenuPrefab, UiLayer.Popup);
            _activePauseMenu.Initialize(false);
            _pauseSubscription = new PauseMenuSubscription(
                _activePauseMenu, OnPauseCloseRequested, OnPauseMenuRequested, OnPauseClosed);
            _uiSpawner.Show(_activePauseMenu);
        }

        private void OnWindowCloseRequested()
        {
            if (_activeWinMenu != null)
            {
                _activeWinMenu.BeginClose();
            }
            else if (_activeFailMenu != null)
            {
                _activeFailMenu.BeginClose();
            }
            else if (_activePauseMenu != null)
            {
                _activePauseMenu.BeginClose();
            }
        }

        private void OnWinMenuClosed()
        {
            ReleaseWinMenu();
            _machine.NotifyWindowClosed();
        }

        private void OnFailMenuClosed()
        {
            ReleaseFailMenu();
            _machine.NotifyWindowClosed();
        }

        private void OnPauseClosed()
        {
            ReleasePauseMenu();
            _machine.NotifyWindowClosed();
        }

        private void OnWinNextLevelRequested()
        {
            _machine.RequestAction(FillUiAction.NextLevel);
        }

        private void OnWinMenuRequested()
        {
            _machine.RequestAction(FillUiAction.Menu);
        }

        private void OnFailRestartRequested()
        {
            _machine.RequestAction(FillUiAction.Restart);
        }

        private void OnFailMenuRequested()
        {
            _machine.RequestAction(FillUiAction.Menu);
        }

        private void OnPauseMenuRequested()
        {
            _machine.RequestAction(FillUiAction.Menu);
        }

        private void OnPauseCloseRequested()
        {
            _machine.RequestAction(FillUiAction.Continue);
        }

        private void OnWinDoubleRewardRequested()
        {
            _machine.RequestDoubleReward();
        }

        private void OnFailRescueRequested()
        {
            _machine.RequestRescue();
        }

        private void OnDoubleRewardAdRequested(Action<bool> complete)
        {
            _adScheduler.ShowDoubleReward(() => complete(true), () => complete(false));
        }

        private void OnRescueAdRequested(Action<bool> complete)
        {
            _adScheduler.ShowRewarded(
                _yandexConfig.FillRescueRewardId,
                () => complete(true),
                () => complete(false),
                () => complete(false),
                () => complete(false));
        }

        private void OnRewardChanged(int amount)
        {
            if (_activeWinMenu != null)
            {
                _activeWinMenu.PlayRewardCountUp(amount);
            }
        }

        private void OnNavigationRequested(FillUiAction action)
        {
            switch (action)
            {
                case FillUiAction.NextLevel:
                case FillUiAction.Restart:
                    _orchestrator.ReloadLevel();
                    break;

                case FillUiAction.Menu:
                    _orchestrator.ExitToMenu();
                    break;

                case FillUiAction.Rescue:
                    _orchestrator.RescueFill();
                    break;
            }
        }

        private void OnOperationFailed(Exception exception)
        {
            UniTask.FromException(exception).Forget();
        }
    }
}
