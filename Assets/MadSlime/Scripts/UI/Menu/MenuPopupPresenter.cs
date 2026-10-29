using System;
using MadSlime.Roulette.Logic;
using MadSlime.UI.Leaderboards;
using MadSlime.UI.Roulette;
using MadSlime.UI.Spawning;
using MadSlime.UI.Windows;

namespace MadSlime.UI.Menu
{
    public class MenuPopupPresenter
    {
        private readonly UiSpawner _spawner;
        private readonly MenuPopupStack _popups;
        private readonly RoulettePreparation _roulettePreparation;
        private readonly PauseMenu _settingsMenuPrefab;
        private readonly LeaderboardMenu _leaderboardMenuPrefab;
        private RoulettePresenter _dailyRoulette;
        private RoulettePresenter _skinRoulette;
        private string _rewardSkinId;
        private PauseMenu _settings;
        private LeaderboardMenu _leaderboard;

        public MenuPopupPresenter(
            UiSpawner spawner,
            MenuPopupStack popups,
            RoulettePreparation roulettePreparation,
            PauseMenu settingsMenuPrefab,
            LeaderboardMenu leaderboardMenuPrefab)
        {
            _spawner = spawner;
            _popups = popups;
            _roulettePreparation = roulettePreparation;
            _settingsMenuPrefab = settingsMenuPrefab;
            _leaderboardMenuPrefab = leaderboardMenuPrefab;
        }

        public event Action ResetRequested;
        public event Action<string> SkinsRefreshRequested;

        public void Stop()
        {
            ReleaseSettings();
            ReleaseLeaderboard();
            ReleaseDailyRoulette();
            ReleaseSkinRoulette();
        }

        public void OpenDailyRoulette()
        {
            _dailyRoulette = _roulettePreparation.SpawnDaily(_popups);
            Subscribe(_dailyRoulette, true);
            _popups.Push(_dailyRoulette);
            _dailyRoulette.Show();
        }

        public void OpenSkinRoulette()
        {
            RouletteModel model = _roulettePreparation.SkinModel;

            if (model == null || model.IsSpinAvailable(false) == false)
            {
                return;
            }

            _rewardSkinId = null;
            _skinRoulette = _roulettePreparation.SpawnSkins(_popups);
            Subscribe(_skinRoulette, false);
            _popups.Push(_skinRoulette);

            if (_skinRoulette.TryShowAndSpin() == false)
            {
                ReleaseSkinRoulette();
                SkinsRefreshRequested?.Invoke(null);
            }
        }

        public void OpenLeaderboard()
        {
            _leaderboard = _spawner.Spawn(_leaderboardMenuPrefab, UiLayer.Popup);
            Subscribe(_leaderboard);
            _leaderboard.Initialize();
            _spawner.Show(_leaderboard);
            _popups.Push(_leaderboard);
        }

        public void OpenSettings()
        {
            _settings = _spawner.Spawn(_settingsMenuPrefab, UiLayer.Popup);
            Subscribe(_settings);
            _settings.Initialize(false, true);
            _spawner.Show(_settings);
            _popups.Push(_settings);
        }

        private void ReleaseDailyRoulette()
        {
            RoulettePresenter popup = _dailyRoulette;

            _dailyRoulette = null;

            if (popup != null)
            {
                Unsubscribe(popup, true);
                ReleaseRoulette(popup);
            }
        }

        private void ReleaseSkinRoulette()
        {
            RoulettePresenter popup = _skinRoulette;

            _skinRoulette = null;

            if (popup != null)
            {
                Unsubscribe(popup, false);
                ReleaseRoulette(popup);
            }
        }

        private void ReleaseRoulette(RoulettePresenter popup)
        {
            _spawner.Release(popup);

            if (_popups.IsTop(popup))
            {
                _popups.Pop(popup);
            }
        }

        private void ReleaseSettings()
        {
            _popups.Release(_settings, Unsubscribe);
            _settings = null;
        }

        private void ReleaseLeaderboard()
        {
            _popups.Release(_leaderboard, Unsubscribe);
            _leaderboard = null;
        }

        private void Unsubscribe(PauseMenu popup)
        {
            popup.CloseButtonClicked -= OnSettingsCloseRequested;
            popup.ResetConfirmed -= OnResetConfirmed;
            popup.Closed -= OnSettingsClosed;
        }

        private void Unsubscribe(LeaderboardMenu popup)
        {
            popup.CloseRequested -= OnLeaderboardCloseRequested;
            popup.Closed -= OnLeaderboardClosed;
        }

        private void Subscribe(PauseMenu popup)
        {
            popup.CloseButtonClicked += OnSettingsCloseRequested;
            popup.ResetConfirmed += OnResetConfirmed;
            popup.Closed += OnSettingsClosed;
        }

        private void Subscribe(LeaderboardMenu popup)
        {
            popup.CloseRequested += OnLeaderboardCloseRequested;
            popup.Closed += OnLeaderboardClosed;
        }

        private void Subscribe(RoulettePresenter popup, bool isDaily)
        {
            if (isDaily)
            {
                popup.Closed += OnDailyRouletteClosed;
                return;
            }

            popup.PrizePresented += OnSkinPrizePresented;
            popup.Closed += OnSkinRouletteClosed;
        }

        private void Unsubscribe(RoulettePresenter popup, bool isDaily)
        {
            if (isDaily)
            {
                popup.Closed -= OnDailyRouletteClosed;
                return;
            }

            popup.PrizePresented -= OnSkinPrizePresented;
            popup.Closed -= OnSkinRouletteClosed;
        }

        private void OnResetConfirmed()
        {
            ResetRequested?.Invoke();
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

        private void OnSettingsCloseRequested()
        {
            _settings.BeginClose();
        }

        private void OnSettingsClosed()
        {
            _popups.Pop(_settings);
            ReleaseSettings();
        }

        private void OnSkinPrizePresented(RoulettePrize prize)
        {
            _rewardSkinId = null;

            if (prize.Skin != null)
            {
                _rewardSkinId = prize.Skin.Id;
            }
        }

        private void OnDailyRouletteClosed()
        {
            ReleaseDailyRoulette();
        }

        private void OnSkinRouletteClosed()
        {
            ReleaseSkinRoulette();
            SkinsRefreshRequested?.Invoke(_rewardSkinId);
        }
    }
}
