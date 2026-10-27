using System;
using Game;
using Roulette;
using UI.Roulette;
using UI.Shop;
using UI.Spawning;
using UnityEngine;

namespace UI.Menu
{
    public class MenuRouletteFlow
    {
        private UiSpawner _spawner;
        private RoulettePreparation _roulettePreparation;
        private MenuPopupStack _popups;
        private MenuContent _content;
        private RoulettePresenter _dailyRoulette;
        private RoulettePresenter _skinRoulette;
        private RouletteWinPopup _prize;
        private string _rewardSkinId;

        public MenuRouletteFlow(UiSpawner spawner, RoulettePreparation roulettePreparation, MenuPopupStack popups, MenuContent content)
        {
            ArgumentGuard.CheckNotNull(spawner, nameof(spawner));
            ArgumentGuard.CheckNotNull(roulettePreparation, nameof(roulettePreparation));
            ArgumentGuard.CheckNotNull(popups, nameof(popups));
            ArgumentGuard.CheckNotNull(content, nameof(content));

            _spawner = spawner;
            _roulettePreparation = roulettePreparation;
            _popups = popups;
            _content = content;
        }

        public void OpenDaily()
        {
            _dailyRoulette = _roulettePreparation.SpawnDaily();
            _dailyRoulette.CloseRequested += OnDailyCloseRequested;
            _dailyRoulette.Closed += OnDailyClosed;
            _dailyRoulette.Show(OnDailySpinCompleted);
            _popups.Push(_dailyRoulette);
        }

        public void OpenSkin()
        {
            if (_roulettePreparation.SkinModel == null)
            {
                return;
            }

            _roulettePreparation.SkinModel.RefreshSlots();

            if (_roulettePreparation.SkinModel.CanSpin(false) == false)
            {
                return;
            }

            _rewardSkinId = null;
            _skinRoulette = _roulettePreparation.SpawnSkins();
            _skinRoulette.Closed += OnSkinRouletteClosed;

            if (_skinRoulette.ShowAndSpin(OnSkinSpinCompleted))
            {
                _popups.Push(_skinRoulette);
            }
            else
            {
                ReleaseSkinRoulette();
                _content.RefreshSkins(null);
            }
        }

        public void ReleaseAll()
        {
            ReleaseDailyRoulette();
            ReleaseSkinRoulette();
            ReleasePrize();
        }

        private void OnDailyCloseRequested()
        {
            if (_popups.IsTop(_dailyRoulette))
            {
                _dailyRoulette.Hide();
            }
        }

        private void OnDailyClosed()
        {
            _popups.Pop(_dailyRoulette);
            ReleaseDailyRoulette();
        }

        private void OnSkinRouletteClosed()
        {
            _popups.Pop(_skinRoulette);
            ReleaseSkinRoulette();
            _content.RefreshSkins(_rewardSkinId);
        }

        private void OnDailySpinCompleted(RoulettePrize prize)
        {
            if (_dailyRoulette == null)
            {
                return;
            }

            ShowPrize(_dailyRoulette.View, prize);
        }

        private void OnSkinSpinCompleted(RoulettePrize prize)
        {
            if (prize.Skin != null)
            {
                _rewardSkinId = prize.Skin.Id;
            }

            if (_skinRoulette == null)
            {
                _content.RefreshSkins(_rewardSkinId);
                return;
            }

            ShowPrize(_skinRoulette.View, prize);
        }

        private void ShowPrize(RouletteView source, RoulettePrize prize)
        {
            _prize = _spawner.Spawn(source.WinPopupPrefab, UiLayer.Popup);
            _prize.Initialize(prize, source);
            _prize.CloseRequested += OnPrizeCloseRequested;
            _prize.Closed += OnPrizeClosed;
            _spawner.Show(_prize);
            _popups.Push(_prize);
        }

        private void OnPrizeCloseRequested()
        {
            if (_popups.IsTop(_prize))
            {
                _prize.BeginClose();
            }
        }

        private void OnPrizeClosed()
        {
            _popups.Pop(_prize);
            ReleasePrize();

            if (_dailyRoulette != null)
            {
                _dailyRoulette.ResumeAfterPrize();
            }
            else if (_skinRoulette != null)
            {
                _skinRoulette.Hide();
            }
        }

        private void ReleaseDailyRoulette()
        {
            _popups.Release(_dailyRoulette, popup =>
            {
                popup.CloseRequested -= OnDailyCloseRequested;
                popup.Closed -= OnDailyClosed;
            });
            _dailyRoulette = null;
        }

        private void ReleaseSkinRoulette()
        {
            _popups.Release(_skinRoulette, popup =>
            {
                popup.Closed -= OnSkinRouletteClosed;
            });
            _skinRoulette = null;
        }

        private void ReleasePrize()
        {
            _popups.Release(_prize, popup =>
            {
                popup.CloseRequested -= OnPrizeCloseRequested;
                popup.Closed -= OnPrizeClosed;
            });
            _prize = null;
        }
    }
}
