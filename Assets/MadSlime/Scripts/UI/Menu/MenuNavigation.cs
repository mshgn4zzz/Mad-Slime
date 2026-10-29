using System;
using Cysharp.Threading.Tasks;
using MadSlime.GameFSM;
using MadSlime.Roulette.Logic;
using MadSlime.UI.Leaderboards;
using MadSlime.UI.Menu.PageStateMachine;
using MadSlime.UI.Shop;
using MadSlime.UI.Spawning;
using MadSlime.UI.Windows;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VContainer;

namespace MadSlime.UI.Menu
{
    public class MenuNavigation : MonoBehaviour
    {
        [SerializeField]
        private GameObject _uiRoot;
        [SerializeField]
        private GameObject _mainWindow;
        [SerializeField]
        private GameObject _skinsWindow;
        [SerializeField]
        private GameObject _upgradesWindow;
        [SerializeField]
        private Camera _previewCamera;
        [SerializeField]
        private Button _playButton;
        [SerializeField]
        private Button _skinsButton;
        [SerializeField]
        private Button _upgradesButton;
        [SerializeField]
        private Button _leaderboardButton;
        [SerializeField]
        private Button _settingsButton;
        [SerializeField]
        private Button _dailyButton;
        [SerializeField]
        private Button _skinsCloseButton;
        [SerializeField]
        private Button _upgradesCloseButton;
        [SerializeField]
        private Button _openSkinButton;
        [SerializeField]
        private PauseMenu _settingsMenuPrefab;
        [SerializeField]
        private LeaderboardMenu _leaderboardMenuPrefab;
        [SerializeField]
        private MenuContent _content;

        private RoulettePreparation _roulettePreparation;
        private MenuPageStateMachine _pages;
        private MenuPopupStack _popups;
        private MenuPopupPresenter _popupPresenter;
        private UiSpawner _spawner;
        private SceneStateMachine _scenes;
        private MenuSessionData _sessionData;
        private ResetProgressFlow _resetProgress;

        public event Action PlayRequested;

        private void Awake()
        {
            if (_uiRoot == null || _mainWindow == null || _skinsWindow == null || _upgradesWindow == null
                || _previewCamera == null
                || _playButton == null
                || _skinsButton == null
                || _upgradesButton == null
                || _leaderboardButton == null
                || _settingsButton == null
                || _dailyButton == null
                || _skinsCloseButton == null
                || _upgradesCloseButton == null
                || _openSkinButton == null
                || _settingsMenuPrefab == null
                || _leaderboardMenuPrefab == null
                || _roulettePreparation == null
                || _content == null)
            {
                throw new InvalidOperationException($"{name}: assign all menu navigation references.");
            }

            if (_spawner == null || _scenes == null || _sessionData == null || _resetProgress == null || _pages == null)
            {
                throw new InvalidOperationException($"{name}: menu navigation dependencies were not injected.");
            }

            _popups = new MenuPopupStack(_spawner, _mainWindow, _skinsWindow, _upgradesWindow);
            _popupPresenter = new MenuPopupPresenter(
                _spawner, _popups, _roulettePreparation, _settingsMenuPrefab, _leaderboardMenuPrefab);
        }

        private void OnEnable()
        {
            _pages.MainRequested += OnMainRequested;
            _pages.SkinsRequested += OnSkinsRequested;
            _pages.UpgradesRequested += OnUpgradesRequested;
            _pages.Stopped += OnPagesStopped;
            _playButton.onClick.AddListener(OnPlayClicked);
            _skinsButton.onClick.AddListener(OnSkinsClicked);
            _upgradesButton.onClick.AddListener(OnUpgradesClicked);
            _leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _dailyButton.onClick.AddListener(OnDailyClicked);
            _skinsCloseButton.onClick.AddListener(OnCollectionCloseClicked);
            _upgradesCloseButton.onClick.AddListener(OnCollectionCloseClicked);
            _openSkinButton.onClick.AddListener(OnOpenSkinClicked);
            _popupPresenter.ResetRequested += OnResetConfirmed;
            _popupPresenter.SkinsRefreshRequested += OnSkinsRefreshRequested;
        }

        private void OnDisable()
        {
            Stop();
            _pages.MainRequested -= OnMainRequested;
            _pages.SkinsRequested -= OnSkinsRequested;
            _pages.UpgradesRequested -= OnUpgradesRequested;
            _pages.Stopped -= OnPagesStopped;
            RemoveListener(_playButton, OnPlayClicked);
            RemoveListener(_skinsButton, OnSkinsClicked);
            RemoveListener(_upgradesButton, OnUpgradesClicked);
            RemoveListener(_leaderboardButton, OnLeaderboardClicked);
            RemoveListener(_settingsButton, OnSettingsClicked);
            RemoveListener(_dailyButton, OnDailyClicked);
            RemoveListener(_skinsCloseButton, OnCollectionCloseClicked);
            RemoveListener(_upgradesCloseButton, OnCollectionCloseClicked);
            RemoveListener(_openSkinButton, OnOpenSkinClicked);
            _popupPresenter.ResetRequested -= OnResetConfirmed;
            _popupPresenter.SkinsRefreshRequested -= OnSkinsRefreshRequested;
        }

        [Inject]
        public void Construct(
            UiSpawner spawner,
            SceneStateMachine scenes,
            MenuSessionData sessionData,
            ResetProgressFlow resetProgress,
            MenuPageStateMachine pages,
            RoulettePreparation roulettePreparation)
        {
            _roulettePreparation = roulettePreparation;
            _pages = pages;
            _spawner = spawner;
            _scenes = scenes;
            _sessionData = sessionData;
            _resetProgress = resetProgress;
        }

        public void Prepare()
        {
            _uiRoot.SetActive(true);
            Stop();
        }

        public void Run()
        {
            if (_pages.IsRunning())
            {
                return;
            }

            _pages.Start();

            if (_sessionData.TryMarkDailyShown())
            {
                _popupPresenter.OpenDailyRoulette();
            }
        }

        public void Stop()
        {
            _pages?.Stop();
            ShowScreens(false, false, false);
            _popupPresenter?.Stop();
            _popups?.Clear();
        }

        private void RemoveListener(Button button, UnityAction listener)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(listener);
            }
        }

        private bool IsOpeningFromMainAllowed()
        {
            return _pages.IsMain() && IsScreenInteractionAllowed();
        }

        private bool IsScreenInteractionAllowed()
        {
            return _popups.IsPopupOpen() == false && _scenes.IsTransitioning == false && _scenes.IsStopped == false
                && _resetProgress.IsRunning == false;
        }

        private void ShowScreens(bool isMain, bool isSkins, bool isUpgrades)
        {
            SetActiveIfAlive(_mainWindow, isMain);
            SetActiveIfAlive(_skinsWindow, isSkins);
            SetActiveIfAlive(_upgradesWindow, isUpgrades);

            if (_previewCamera != null)
            {
                SetActiveIfAlive(_previewCamera.gameObject, isSkins);
            }
        }

        private void SetActiveIfAlive(GameObject target, bool isActive)
        {
            if (target != null)
            {
                target.SetActive(isActive);
            }
        }

        private void OnPlayClicked()
        {
            if (IsOpeningFromMainAllowed())
            {
                PlayRequested?.Invoke();
            }
        }

        private void OnSkinsClicked()
        {
            if (IsOpeningFromMainAllowed() == false)
            {
                return;
            }

            _pages.OpenSkins();
        }

        private void OnUpgradesClicked()
        {
            if (IsOpeningFromMainAllowed())
            {
                _pages.OpenUpgrades();
            }
        }

        private void OnCollectionCloseClicked()
        {
            if (IsScreenInteractionAllowed() && (_pages.IsCollection()))
            {
                _pages.NavigateBack();
            }
        }

        private void OnLeaderboardClicked()
        {
            if (IsOpeningFromMainAllowed() == false)
            {
                return;
            }

            _popupPresenter.OpenLeaderboard();
        }

        private void OnSettingsClicked()
        {
            if (IsOpeningFromMainAllowed() == false)
            {
                return;
            }

            _popupPresenter.OpenSettings();
        }

        private void OnSkinsRefreshRequested(string skinId)
        {
            _content.RefreshSkins(skinId);
        }

        private void OnResetConfirmed()
        {
            _resetProgress.RunAsync(this.GetCancellationTokenOnDestroy()).Forget(_scenes.HandleTransitionException);
        }

        private void OnDailyClicked()
        {
            if (IsOpeningFromMainAllowed())
            {
                _popupPresenter.OpenDailyRoulette();
            }
        }

        private void OnOpenSkinClicked()
        {
            if (_pages.IsSkins() == false || IsScreenInteractionAllowed() == false)
            {
                return;
            }

            _popupPresenter.OpenSkinRoulette();
        }

        private void OnMainRequested()
        {
            ShowScreens(true, false, false);
        }

        private void OnSkinsRequested()
        {
            ShowScreens(false, true, false);
            _content.RefreshSkins(null);
        }

        private void OnUpgradesRequested()
        {
            ShowScreens(false, false, true);
        }

        private void OnPagesStopped()
        {
            ShowScreens(false, false, false);
        }
    }
}
