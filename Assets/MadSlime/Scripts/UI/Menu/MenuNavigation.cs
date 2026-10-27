using System;
using Cysharp.Threading.Tasks;
using Game;
using Roulette;
using UI.Leaderboards;
using UI.Roulette;
using UI.Shop;
using UI.Spawning;
using UI.Windows;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VContainer;

namespace UI.Menu
{
    public class MenuNavigation : MonoBehaviour
    {
        private enum WindowState
        {
            Stopped,
            Main,
            Skins,
            Upgrades
        }

        [SerializeField] private GameObject _uiRoot;
        [SerializeField] private GameObject _mainWindow;
        [SerializeField] private GameObject _skinsWindow;
        [SerializeField] private GameObject _upgradesWindow;
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
        [SerializeField] private RoulettePreparation _roulettePreparation;
        [SerializeField] private MenuContent _content;

        private WindowState _state;
        private MenuPopupStack _popups;
        private MenuRouletteFlow _rouletteFlow;
        private UiSpawner _spawner;
        private SceneNavigator _sceneNavigator;
        private PauseMenu _settings;
        private LeaderboardMenu _leaderboard;
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
            if (_uiRoot == null || _mainWindow == null || _skinsWindow == null || _upgradesWindow == null
                || _previewCamera == null || _playButton == null || _skinsButton == null || _upgradesButton == null
                || _leaderboardButton == null || _settingsButton == null || _dailyButton == null
                || _skinsCloseButton == null || _upgradesCloseButton == null || _openSkinButton == null
                || _settingsMenuPrefab == null || _leaderboardMenuPrefab == null
                || _roulettePreparation == null || _content == null)
            {
                throw new InvalidOperationException($"{name}: assign all menu navigation references.");
            }

            if (_spawner == null || _sceneNavigator == null)
            {
                throw new InvalidOperationException($"{name}: menu navigation dependencies were not injected.");
            }

            _popups = new MenuPopupStack(_spawner, _mainWindow, _skinsWindow, _upgradesWindow);
            _rouletteFlow = new MenuRouletteFlow(_spawner, _roulettePreparation, _popups, _content);
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
                _rouletteFlow.OpenDaily();
            }
        }

        public void Stop()
        {
            Unsubscribe();
            SetWindow(WindowState.Stopped);
            ReleaseSettings();
            ReleaseLeaderboard();
            _rouletteFlow?.ReleaseAll();
            _popups?.Clear();
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
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            RemoveListener(_playButton, OnPlayClicked);
            RemoveListener(_skinsButton, OnSkinsClicked);
            RemoveListener(_upgradesButton, OnUpgradesClicked);
            RemoveListener(_leaderboardButton, OnLeaderboardClicked);
            RemoveListener(_settingsButton, OnSettingsClicked);
            RemoveListener(_dailyButton, OnDailyClicked);
            RemoveListener(_skinsCloseButton, OnCollectionCloseClicked);
            RemoveListener(_upgradesCloseButton, OnCollectionCloseClicked);
            RemoveListener(_openSkinButton, OnOpenSkinClicked);
            _isSubscribed = false;
        }

        private static void RemoveListener(Button button, UnityAction listener)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(listener);
            }
        }

        private bool CanOpenFromMain()
        {
            return _state == WindowState.Main && CanInteractWithScreen();
        }

        private bool CanInteractWithScreen()
        {
            return _popups.HasOpenPopups() == false && _sceneNavigator.IsTransitioning == false;
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
            if (CanInteractWithScreen() && (_state == WindowState.Skins || _state == WindowState.Upgrades))
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
            _leaderboard.Initialize();
            _spawner.Show(_leaderboard);
            _popups.Push(_leaderboard);
        }

        private void OnLeaderboardCloseRequested()
        {
            _leaderboard.BeginClose();
        }

        private void OnLeaderboardClosed()
        {
            _popups.Pop(_leaderboard);
            ReleaseLeaderboard();
        }

        private void OnSettingsClicked()
        {
            if (CanOpenFromMain() == false)
            {
                return;
            }

            _settings = _spawner.Spawn(_settingsMenuPrefab, UiLayer.Popup);
            _settings.CloseButtonClicked += OnSettingsCloseRequested;
            _settings.ResetConfirmed += OnResetConfirmed;
            _settings.Closed += OnSettingsClosed;
            _settings.Initialize(false, true);
            _spawner.Show(_settings);
            _popups.Push(_settings);
        }

        private void OnSettingsCloseRequested()
        {
            _settings.BeginClose();
        }

        private void OnResetConfirmed()
        {
            _sceneNavigator.ResetProgressAndLoadMenuAsync().Forget();
        }

        private void OnSettingsClosed()
        {
            _popups.Pop(_settings);
            ReleaseSettings();
        }

        private void OnDailyClicked()
        {
            if (CanOpenFromMain() == true)
            {
                _rouletteFlow.OpenDaily();
            }
        }

        private void OnOpenSkinClicked()
        {
            if (_state != WindowState.Skins || CanInteractWithScreen() == false)
            {
                return;
            }

            _rouletteFlow.OpenSkin();
        }

        private void SetWindow(WindowState state)
        {
            _state = state;
            SetActiveIfAlive(_mainWindow, state == WindowState.Main);
            SetActiveIfAlive(_skinsWindow, state == WindowState.Skins);
            SetActiveIfAlive(_upgradesWindow, state == WindowState.Upgrades);

            if (_previewCamera != null)
            {
                SetActiveIfAlive(_previewCamera.gameObject, state == WindowState.Skins);
            }
        }

        private static void SetActiveIfAlive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private void ReleaseSettings()
        {
            _popups.Release(_settings, popup =>
            {
                popup.CloseButtonClicked -= OnSettingsCloseRequested;
                popup.ResetConfirmed -= OnResetConfirmed;
                popup.Closed -= OnSettingsClosed;
            });
            _settings = null;
        }

        private void ReleaseLeaderboard()
        {
            _popups.Release(_leaderboard, popup =>
            {
                popup.CloseRequested -= OnLeaderboardCloseRequested;
                popup.Closed -= OnLeaderboardClosed;
            });
            _leaderboard = null;
        }
    }
}
