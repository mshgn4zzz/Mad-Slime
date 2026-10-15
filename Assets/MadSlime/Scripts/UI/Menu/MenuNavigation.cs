using System;
using Game;
using Roulette;
using Shop;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI
{
    public class MenuNavigation : MonoBehaviour
    {
        private enum WindowState
        {
            Stopped,
            Main,
            Skins,
            Upgrades,
            Settings,
            Leaderboard,
            DailyRoulette,
            SkinRoulette,
            Prize
        }

        [SerializeField] private GameObject _uiRoot;
        [SerializeField] private GameObject _backgroundRoot;
        [SerializeField] private GameObject _mainWindow;
        [SerializeField] private GameObject _skinsWindow;
        [SerializeField] private GameObject _upgradesWindow;
        [SerializeField] private GameObject _dailyWindow;
        [SerializeField] private GameObject _skinRouletteWindow;
        [SerializeField] private Camera _previewCamera;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _skinsButton;
        [SerializeField] private Button _upgradesButton;
        [SerializeField] private Button _leaderboardButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _dailyButton;
        [SerializeField] private Button _skinsCloseButton;
        [SerializeField] private Button _upgradesCloseButton;
        [SerializeField] private Button _openSkinButton;
        [SerializeField] private PauseMenu _settingsMenuPrefab;
        [SerializeField] private LeaderboardMenu _leaderboardMenuPrefab;
        [SerializeField] private RoulettePresenter _dailyRoulette;
        [SerializeField] private RoulettePresenter _skinRoulette;
        [SerializeField] private MenuContent _content;

        private WindowState _state;
        private WindowState _prizeReturnWindow;
        private UiSpawner _spawner;
        private SceneNavigator _sceneNavigator;
        private PauseMenu _settings;
        private LeaderboardMenu _leaderboard;
        private RouletteWinPopup _prize;
        private string _rewardSkinId;
        private bool _isSubscribed;

        public event Action PlayRequested;

        [Inject]
        public void Construct(UiSpawner spawner, SceneNavigator sceneNavigator)
        {
            _spawner = spawner;
            _sceneNavigator = sceneNavigator;
        }

        private void Awake()
        {
            if (_uiRoot == null || _backgroundRoot == null || _mainWindow == null || _skinsWindow == null
                || _upgradesWindow == null || _dailyWindow == null || _skinRouletteWindow == null
                || _previewCamera == null || _playButton == null || _skinsButton == null || _upgradesButton == null
                || _leaderboardButton == null || _settingsButton == null || _dailyButton == null
                || _skinsCloseButton == null || _upgradesCloseButton == null || _openSkinButton == null
                || _settingsMenuPrefab == null || _leaderboardMenuPrefab == null || _dailyRoulette == null
                || _skinRoulette == null || _content == null)
            {
                throw new InvalidOperationException($"{name}: assign all menu navigation references.");
            }

            if (_spawner == null || _sceneNavigator == null)
            {
                throw new InvalidOperationException($"{name}: menu navigation dependencies were not injected.");
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Prepare()
        {
            _uiRoot.SetActive(true);
            Stop();
        }

        public void Run()
        {
            if (_state != WindowState.Stopped)
            {
                return;
            }

            Subscribe();
            SetWindow(WindowState.Main);

            if (_sceneNavigator.TryMarkDailyShown() == true)
            {
                OpenDailyRoulette();
            }
        }

        public void Stop()
        {
            Unsubscribe();
            SetWindow(WindowState.Stopped);
            ReleaseSettings();
            ReleaseLeaderboard();
            ReleasePrize();
        }

        private void Subscribe()
        {
            if (_isSubscribed == true)
            {
                return;
            }

            _playButton.onClick.AddListener(OnPlayClicked);
            _skinsButton.onClick.AddListener(OnSkinsClicked);
            _upgradesButton.onClick.AddListener(OnUpgradesClicked);
            _leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _dailyButton.onClick.AddListener(OnDailyClicked);
            _skinsCloseButton.onClick.AddListener(OnCollectionCloseClicked);
            _upgradesCloseButton.onClick.AddListener(OnCollectionCloseClicked);
            _openSkinButton.onClick.AddListener(OnOpenSkinClicked);
            _dailyRoulette.CloseRequested += OnDailyCloseRequested;
            _dailyRoulette.Closed += OnDailyClosed;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            _playButton.onClick.RemoveListener(OnPlayClicked);
            _skinsButton.onClick.RemoveListener(OnSkinsClicked);
            _upgradesButton.onClick.RemoveListener(OnUpgradesClicked);
            _leaderboardButton.onClick.RemoveListener(OnLeaderboardClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            _dailyButton.onClick.RemoveListener(OnDailyClicked);
            _skinsCloseButton.onClick.RemoveListener(OnCollectionCloseClicked);
            _upgradesCloseButton.onClick.RemoveListener(OnCollectionCloseClicked);
            _openSkinButton.onClick.RemoveListener(OnOpenSkinClicked);
            _dailyRoulette.CloseRequested -= OnDailyCloseRequested;
            _dailyRoulette.Closed -= OnDailyClosed;
            _isSubscribed = false;
        }

        private bool CanOpenFromMain()
        {
            return _state == WindowState.Main && _sceneNavigator.IsTransitioning == false;
        }

        private void OnPlayClicked()
        {
            if (CanOpenFromMain() == true)
            {
                PlayRequested?.Invoke();
            }
        }

        private void OnSkinsClicked()
        {
            if (CanOpenFromMain() == false)
            {
                return;
            }

            SetWindow(WindowState.Skins);
            _content.RefreshSkins(null);
        }

        private void OnUpgradesClicked()
        {
            if (CanOpenFromMain() == true)
            {
                SetWindow(WindowState.Upgrades);
            }
        }

        private void OnCollectionCloseClicked()
        {
            if (_state == WindowState.Skins || _state == WindowState.Upgrades)
            {
                SetWindow(WindowState.Main);
            }
        }

        private void OnLeaderboardClicked()
        {
            if (CanOpenFromMain() == false)
            {
                return;
            }

            _leaderboard = _spawner.Spawn(_leaderboardMenuPrefab, UiLayer.Popup);
            _leaderboard.CloseRequested += OnLeaderboardCloseRequested;
            _leaderboard.Closed += OnLeaderboardClosed;
            SetWindow(WindowState.Leaderboard);
            _leaderboard.Initialize();
            _spawner.Show(_leaderboard);
        }

        private void OnLeaderboardCloseRequested()
        {
            _leaderboard.BeginClose();
        }

        private void OnLeaderboardClosed()
        {
            ReleaseLeaderboard();
            SetWindow(WindowState.Main);
        }

        private void OnSettingsClicked()
        {
            if (CanOpenFromMain() == false)
            {
                return;
            }

            _settings = _spawner.Spawn(_settingsMenuPrefab, UiLayer.Popup);
            _settings.CloseButtonClicked += OnSettingsCloseRequested;
            _settings.Closed += OnSettingsClosed;
            SetWindow(WindowState.Settings);
            _settings.Initialize(false);
            _spawner.Show(_settings);
        }

        private void OnSettingsCloseRequested()
        {
            _settings.BeginClose();
        }

        private void OnSettingsClosed()
        {
            ReleaseSettings();
            SetWindow(WindowState.Main);
        }

        private void OnDailyClicked()
        {
            if (CanOpenFromMain() == true)
            {
                OpenDailyRoulette();
            }
        }

        private void OpenDailyRoulette()
        {
            SetWindow(WindowState.DailyRoulette);
            _dailyRoulette.Show(OnDailySpinCompleted);
        }

        private void OnDailyCloseRequested()
        {
            if (_state == WindowState.DailyRoulette)
            {
                _dailyRoulette.Hide();
            }
        }

        private void OnDailyClosed()
        {
            if (_state == WindowState.DailyRoulette)
            {
                SetWindow(WindowState.Main);
            }
        }

        private void OnOpenSkinClicked()
        {
            if (_state != WindowState.Skins || _skinRoulette.CanSpin() == false)
            {
                return;
            }

            _rewardSkinId = null;
            SetWindow(WindowState.SkinRoulette);

            if (_skinRoulette.ShowAndSpin(OnSkinSpinCompleted) == false)
            {
                SetWindow(WindowState.Skins);
                _content.RefreshSkins(null);
            }
        }

        private void OnDailySpinCompleted(RoulettePrize prize)
        {
            _prizeReturnWindow = WindowState.DailyRoulette;
            ShowPrize(_dailyRoulette.View, prize);
        }

        private void OnSkinSpinCompleted(RoulettePrize prize)
        {
            _prizeReturnWindow = WindowState.Skins;

            if (prize.Skin != null)
            {
                _rewardSkinId = prize.Skin.Id;
            }

            ShowPrize(_skinRoulette.View, prize);
        }

        private void ShowPrize(RouletteView source, RoulettePrize prize)
        {
            _prize = _spawner.Spawn(source.WinPopupPrefab, UiLayer.Popup);

            _prize.Initialize(prize, source);

            _prize.CloseRequested += OnPrizeCloseRequested;
            _prize.Closed += OnPrizeClosed;
            SetWindow(WindowState.Prize);
            _spawner.Show(_prize);
        }

        private void OnPrizeCloseRequested()
        {
            if (_state == WindowState.Prize)
            {
                _prize.BeginClose();
            }
        }

        private void OnPrizeClosed()
        {
            ReleasePrize();
            SetWindow(_prizeReturnWindow);

            if (_prizeReturnWindow == WindowState.DailyRoulette)
            {
                _dailyRoulette.ResumeAfterPrize();
            }

            if (_prizeReturnWindow == WindowState.Skins)
            {
                _content.RefreshSkins(_rewardSkinId);
            }
        }

        private void SetWindow(WindowState state)
        {
            _state = state;
            DeactivateWindow(_mainWindow);
            DeactivateWindow(_skinsWindow);
            DeactivateWindow(_upgradesWindow);
            if (state != WindowState.DailyRoulette
                && (state != WindowState.Prize || _prizeReturnWindow != WindowState.DailyRoulette))
            {
                DeactivateWindow(_dailyWindow);
            }
            DeactivateWindow(_skinRouletteWindow);
            if (_previewCamera != null)
            {
                DeactivateWindow(_previewCamera.gameObject);
            }

            if (_settings != null)
            {
                _settings.gameObject.SetActive(false);
            }

            if (_leaderboard != null)
            {
                _leaderboard.gameObject.SetActive(false);
            }

            if (_prize != null)
            {
                _prize.gameObject.SetActive(false);
            }

            if (_backgroundRoot != null)
            {
                _backgroundRoot.SetActive(state != WindowState.Stopped);
            }

            switch (state)
            {
                case WindowState.Main:
                    _mainWindow.SetActive(true);
                    break;
                case WindowState.Skins:
                    _previewCamera.gameObject.SetActive(true);
                    _skinsWindow.SetActive(true);
                    break;
                case WindowState.Upgrades:
                    _upgradesWindow.SetActive(true);
                    break;
                case WindowState.Settings:
                    _settings.gameObject.SetActive(true);
                    break;
                case WindowState.Leaderboard:
                    _leaderboard.gameObject.SetActive(true);
                    break;
                case WindowState.DailyRoulette:
                    _dailyWindow.SetActive(true);
                    break;
                case WindowState.SkinRoulette:
                    _skinRouletteWindow.SetActive(true);
                    break;
                case WindowState.Prize:
                    _prize.gameObject.SetActive(true);
                    break;
                case WindowState.Stopped:
                    break;
            }
        }

        private void DeactivateWindow(GameObject window)
        {
            if (window != null)
            {
                window.SetActive(false);
            }
        }

        private void ReleaseSettings()
        {
            if (_settings == null)
            {
                return;
            }

            _settings.CloseButtonClicked -= OnSettingsCloseRequested;
            _settings.Closed -= OnSettingsClosed;
            _spawner.Release(_settings);
            _settings = null;
        }

        private void ReleaseLeaderboard()
        {
            if (_leaderboard == null)
            {
                return;
            }

            _leaderboard.CloseRequested -= OnLeaderboardCloseRequested;
            _leaderboard.Closed -= OnLeaderboardClosed;
            _spawner.Release(_leaderboard);
            _leaderboard = null;
        }

        private void ReleasePrize()
        {
            if (_prize == null)
            {
                return;
            }

            _prize.CloseRequested -= OnPrizeCloseRequested;
            _prize.Closed -= OnPrizeClosed;
            _spawner.Release(_prize);
            _prize = null;
        }
    }
}
