using System;

namespace MadSlime.ShapeFill.Logic.CubeFlightStateMachine.States
{
    public class FlyingCubeState : ICubeFlightState
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
