namespace Leaderboards
{
    public class LeaderboardData
    {
        private LeaderboardEntryData[] _players;
        private LeaderboardEntryData? _currentPlayer;
        private bool _hasPlayerError;

        public LeaderboardEntryData[] Players => _players;

        public LeaderboardEntryData? CurrentPlayer => _currentPlayer;

        public bool HasPlayerError => _hasPlayerError;

        public LeaderboardData(LeaderboardEntryData[] players, LeaderboardEntryData? currentPlayer,
            bool hasPlayerError = false)
        {
            _players = players;
            _currentPlayer = currentPlayer;
            _hasPlayerError = hasPlayerError;
        }
    }
}
