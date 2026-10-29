namespace MadSlime.Leaderboards.Data
{
    public class LeaderboardData
    {
        private LeaderboardEntryData[] _players;
        private LeaderboardEntryData? _currentPlayer;
        private bool _isPlayerError;

        public LeaderboardData(
            LeaderboardEntryData[] players,
            LeaderboardEntryData? currentPlayer,
            bool isPlayerError = false)
        {
            _players = players;
            _currentPlayer = currentPlayer;
            _isPlayerError = isPlayerError;
        }

        public LeaderboardEntryData[] Players => _players;

        public LeaderboardEntryData? CurrentPlayer => _currentPlayer;

        public bool IsPlayerError => _isPlayerError;
    }
}
