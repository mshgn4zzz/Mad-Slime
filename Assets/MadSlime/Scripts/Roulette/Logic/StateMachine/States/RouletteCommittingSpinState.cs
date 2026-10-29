using System;

namespace MadSlime.Roulette.Logic.StateMachine.States
{
    public class RouletteCommittingSpinState : IRouletteState
    {
        private readonly RouletteModel _model;
        private readonly RouletteSessionData _session;

        public RouletteCommittingSpinState(RouletteModel model, RouletteSessionData session)
        {
            _model = model;
            _session = session;
        }

        public event Action Committed;
        public event Action Rejected;
        public event Action<Exception> Failed;

        public void Enter()
        {
            RouletteOutcome outcome;
            bool isAccepted;

            try
            {
                isAccepted = _model.TrySpin(_session.IsAdvertisementGranted, out outcome);
            }
            catch (Exception exception)
            {
                Failed?.Invoke(exception);
                return;
            }

            if (isAccepted == false)
            {
                Rejected?.Invoke();
                return;
            }

            _session.Outcome = outcome;
            Committed?.Invoke();
        }

        public void Exit()
        {
        }
    }
}
