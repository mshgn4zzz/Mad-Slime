using System;

namespace MadSlime.Leaderboards.Adapters
{
    [Serializable]
    public class LeaderboardResponse
    {
        public int RequestId;
        public string Error;
        public LeaderboardPlayerResponse[] Players;
    }
}
