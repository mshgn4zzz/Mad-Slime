using System;

namespace MadSlime.UI.Fill.StateMachine.States
{
    public class RescueAdState : IFillUiState
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
            if (ReferenceEquals(entry, _entry) == false)
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
