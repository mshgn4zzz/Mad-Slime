using System;
using System.Threading;
using Ads;
using Cysharp.Threading.Tasks;
using Leaderboards;
using Saves;

public class FsmAdsService : IAdsService
{
    public static int Outcome;
    public event Action RewardedOpened;
    public event Action<string> RewardReceived;
    public event Action RewardedClosed;
    public event Action RewardedError;
    public event Action<bool> PauseGameChanged;
    public bool IsAdShowing { get; private set; }
    public bool IsPauseGame { get; private set; }

    public void ShowRewarded(string rewardId)
    {
        IsAdShowing = true;
        IsPauseGame = true;
        PauseGameChanged?.Invoke(true);
        RewardedOpened?.Invoke();
        IsAdShowing = false;
        IsPauseGame = false;
        PauseGameChanged?.Invoke(false);
        if (Outcome == 2)
        {
            RewardedError?.Invoke();
            return;
        }
        if (Outcome == 0)
        {
            RewardReceived?.Invoke(rewardId);
        }
        RewardedClosed?.Invoke();
    }

    public void ShowInterstitial()
    {
    }
}

public class FsmSaveConfirmation : ISaveConfirmation
{
    public static bool FailNext;
    public static bool WaitNext;
    private ISaveConfirmation _inner;

    public FsmSaveConfirmation(ISaveConfirmation inner)
    {
        _inner = inner;
    }

    public UniTask ConfirmSavedAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (FailNext)
        {
            FailNext = false;
            throw new InvalidOperationException("Simulated cloud confirmation failure.");
        }
        if (WaitNext)
        {
            WaitNext = false;
            return UniTask.Delay(TimeSpan.FromSeconds(60), cancellationToken: cancellationToken);
        }
        return _inner.ConfirmSavedAsync(cancellationToken);
    }
}

public class FsmLeaderboardService : ILeaderboardService
{
    public static int Reports;
    public static int LastScore;
    public bool IsAuthorized => true;
    public string PlayerId => "fsm-verification";

    public void SetScore(string leaderboardName, int score)
    {
        Reports++;
        LastScore = score;
    }

    public UniTask<LeaderboardData> RequestEntriesAsync(string leaderboardName, int topCount,
        CancellationToken cancellationToken)
    {
        return UniTask.FromResult(new LeaderboardData(Array.Empty<LeaderboardEntryData>(), null));
    }

    public void OpenAuthDialog()
    {
    }
}
