using System;

namespace MadSlime.UI.Leaderboards.StateMachine.States
{
    public class ReadyLeaderboardState : ILeaderboardState
    {
        public event Action Entered;

        public void Enter()
        {
            Entered?.Invoke();
        }

        public void Exit()
        {
        }
    }
}
