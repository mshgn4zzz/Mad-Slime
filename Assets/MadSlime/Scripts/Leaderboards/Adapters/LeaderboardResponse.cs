using System;
namespace Leaderboards
{
    [Serializable]
    public class LeaderboardResponse
    {
        public int requestId;
        public string error;
        public LeaderboardPlayerResponse[] players;
    }
}
