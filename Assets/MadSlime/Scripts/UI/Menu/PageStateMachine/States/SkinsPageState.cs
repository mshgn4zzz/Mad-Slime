using System;

namespace MadSlime.UI.Menu.PageStateMachine.States
{
    public class SkinsPageState : IMenuPageState
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
