using System;

namespace MadSlime.Roulette.Logic.StateMachine.States
{
    public class RouletteWaitingForAdState : IRouletteState
    {
        private object _entry;

        public event Action<Action<bool>> Requested;
        public event Action Granted;
        public event Action Rejected;

        public void Enter()
        {
            object entry = new object();

            _entry = entry;
            Requested?.Invoke(isGranted => Complete(entry, isGranted));
        }

        public void Exit()
        {
            _entry = null;
        }

        private void Complete(object entry, bool isGranted)
        {
            if (ReferenceEquals(_entry, entry) == false)
            {
                return;
            }

            _entry = null;

            if (isGranted)
            {
                Granted?.Invoke();
            }
            else
            {
                Rejected?.Invoke();
            }
        }
    }
}
