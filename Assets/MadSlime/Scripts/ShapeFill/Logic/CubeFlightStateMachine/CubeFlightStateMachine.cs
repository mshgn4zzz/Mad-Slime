using System;
using MadSlime.ShapeFill.Logic.CubeFlightStateMachine.States;
using MadSlime.StateMachine.Logic;

namespace MadSlime.ShapeFill.Logic.CubeFlightStateMachine
{
    public class CubeFlightStateMachine : BaseStateMachine<ICubeFlightState>, IDisposable
    {
        private readonly IdleCubeState _idle = new IdleCubeState();
        private readonly FlyingCubeState _flying = new FlyingCubeState();
        private CubeFlightData _flight;

        public CubeFlightStateMachine()
        {
            RegisterState(_idle);
            RegisterState(_flying);
            _flying.Entered += OnFlying;
            _idle.Entered += OnIdle;
        }

        public event Action<CubeFlightData> FlightRequested;
        public event Action FlightStopped;
        public event Action Arrived;

        public bool IsFlying()
        {
            return CurrentState == _flying;
        }

        public void Launch(CubeFlightData data)
        {
            if (IsFlying())
            {
                Stop();
            }

            _flight = data;
            SwitchState<FlyingCubeState>();
        }

        public void CompleteFlight()
        {
            if (IsFlying() == false)
            {
                return;
            }

            SwitchState<IdleCubeState>();
            Arrived?.Invoke();
        }

        public override void Stop()
        {
            base.Stop();
            _flight = null;
            FlightStopped?.Invoke();
        }

        public void Dispose()
        {
            _flying.Entered -= OnFlying;
            _idle.Entered -= OnIdle;
            base.Stop();
        }

        private void OnFlying()
        {
            FlightRequested?.Invoke(_flight);
        }

        private void OnIdle()
        {
            FlightStopped?.Invoke();
        }
    }
}
