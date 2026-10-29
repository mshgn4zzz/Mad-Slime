using System;

namespace MadSlime.UI.Fill.StateMachine.States
{
    public class WinFillUiState : IFillUiState
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
