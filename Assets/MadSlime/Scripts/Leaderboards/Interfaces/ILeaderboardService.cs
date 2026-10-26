using Cysharp.Threading.Tasks;
using System.Threading;
namespace Leaderboards
{
    public interface ILeaderboardService
    {
        bool IsAuthorized { get; }

        string PlayerId { get; }

        void SetScore(string leaderboardName, int score);

        UniTask<LeaderboardData> RequestEntriesAsync(
            string leaderboardName, int topCount, CancellationToken cancellationToken);

        void OpenAuthDialog();
    }
}
