using System;

namespace MadSlime.Leaderboards.Adapters
{
    [Serializable]
    public class LeaderboardPlayerResponse
    {
        public string Id;
        public string Name;
        public int Rank;
        public int Score;
    }
}
