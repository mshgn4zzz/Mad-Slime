using System;

namespace MadSlime.Roulette.Logic.StateMachine.States
{
    public class RouletteReadyState : IRouletteState
    {
        private readonly RouletteModel _model;
        private readonly RouletteSessionData _session;

        public RouletteReadyState(RouletteModel model, RouletteSessionData session)
        {
            _model = model;
            _session = session;
        }

        public event Action Entered;

        public void Enter()
        {
            _session.Clear();
            _model.RefreshSlots();
            Entered?.Invoke();
        }

        public void Exit()
        {
        }
    }
}
