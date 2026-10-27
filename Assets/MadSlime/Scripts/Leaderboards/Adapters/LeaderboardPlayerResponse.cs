using System;

namespace Leaderboards
{
    [Serializable]
    public class LeaderboardPlayerResponse
    {
        public string id;
        public string name;
        public int rank;
        public int score;
    }
}
