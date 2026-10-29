using System;

namespace MadSlime.Roulette.Logic.StateMachine.States
{
    public class RouletteSaveFailedState : IRouletteState
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
