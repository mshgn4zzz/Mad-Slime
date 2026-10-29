using System;

namespace MadSlime.Game.Logic.TimerStateMachine.States
{
    public class CompletedTimerState : ITimerState
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
