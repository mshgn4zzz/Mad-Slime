using System;

namespace MadSlime.UI.Leaderboards.StateMachine.States
{
    public class EmptyLeaderboardState : ILeaderboardState
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
