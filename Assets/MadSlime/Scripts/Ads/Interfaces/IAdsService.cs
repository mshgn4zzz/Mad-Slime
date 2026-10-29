using System;

namespace MadSlime.Ads.Interfaces
{
    public interface IAdsService
    {
        event Action RewardedOpened;

        event Action<string> RewardReceived;

        event Action RewardedClosed;

        event Action RewardedFailed;

        event Action<bool> PauseGameChanged;

        bool IsAdShowing { get; }

        bool IsPauseGame { get; }

        void ShowRewarded(string rewardId);

        void ShowInterstitial();
    }
}
