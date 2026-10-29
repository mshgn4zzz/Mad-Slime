using System;

namespace MadSlime.UI.Windows.StateMachine.States
{
    public class VisibleWindowState : IWindowLifecycleState
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
