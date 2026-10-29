using System;

namespace MadSlime.Roulette.Logic.StateMachine.States
{
    public class RouletteResultState : IRouletteState
    {
        private readonly RouletteSessionData _session;

        public RouletteResultState(RouletteSessionData session)
        {
            _session = session;
        }

        public event Action<RouletteOutcome> ResultPrepared;

        public void Enter()
        {
            ResultPrepared?.Invoke(_session.Outcome);
        }

        public void Exit()
        {
        }
    }
}
