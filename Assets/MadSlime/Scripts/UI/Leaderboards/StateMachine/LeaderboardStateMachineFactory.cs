using MadSlime.Leaderboards.Interfaces;
using MadSlime.Saves.Interfaces;

namespace MadSlime.UI.Leaderboards.StateMachine
{
    public class LeaderboardStateMachineFactory
    {
        private readonly ISavesReadiness _readiness;
        private readonly ILeaderboardService _service;

        public LeaderboardStateMachineFactory(ISavesReadiness readiness, ILeaderboardService service)
        {
            _readiness = readiness;
            _service = service;
        }

        public LeaderboardStateMachine Create(string name)
        {
            return new LeaderboardStateMachine(_readiness, _service, name);
        }
    }
}
