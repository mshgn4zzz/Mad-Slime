using System;

namespace MadSlime.UI.Menu.PageStateMachine.States
{
    public class UpgradesPageState : IMenuPageState
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
