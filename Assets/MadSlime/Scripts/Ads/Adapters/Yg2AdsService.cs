using System;
using MadSlime.Ads.Interfaces;
using YG;

namespace MadSlime.Ads.Adapters
{
    public class Yg2AdsService : IAdsService, IDisposable
    {
        public Yg2AdsService()
        {
            Subscribe();
        }

        public event Action RewardedOpened;

        public event Action<string> RewardReceived;

        public event Action RewardedClosed;

        public event Action RewardedFailed;

        public event Action<bool> PauseGameChanged;

        public bool IsAdShowing => YG2.nowAdsShow;

        public bool IsPauseGame => YG2.isPauseGame;

        public void Dispose()
        {
            Unsubscribe();
        }

        public void ShowRewarded(string rewardId)
        {
            YG2.RewardedAdvShow(rewardId);
        }

        public void ShowInterstitial()
        {
            YG2.InterstitialAdvShow();
        }

        private void Subscribe()
        {
            YG2.onOpenRewardedAdv += OnRewardedOpened;
            YG2.onRewardAdv += OnRewardReceived;
            YG2.onCloseRewardedAdv += OnRewardedClosed;
            YG2.onErrorRewardedAdv += OnRewardedError;
            YG2.onPauseGame += OnPauseGame;
        }

        private void Unsubscribe()
        {
            YG2.onOpenRewardedAdv -= OnRewardedOpened;
            YG2.onRewardAdv -= OnRewardReceived;
            YG2.onCloseRewardedAdv -= OnRewardedClosed;
            YG2.onErrorRewardedAdv -= OnRewardedError;
            YG2.onPauseGame -= OnPauseGame;
        }

        private void OnRewardedOpened()
        {
            RewardedOpened?.Invoke();
        }

        private void OnRewardReceived(string rewardId)
        {
            RewardReceived?.Invoke(rewardId);
        }

        private void OnRewardedClosed()
        {
            RewardedClosed?.Invoke();
        }

        private void OnRewardedError()
        {
            RewardedFailed?.Invoke();
        }

        private void OnPauseGame(bool isPaused)
        {
            PauseGameChanged?.Invoke(isPaused);
        }
    }
}
