using System;

namespace MadSlime.ShapeFill.Logic.PlaybackStateMachine.States
{
    public class BoostedFillState : IShapeFillPlaybackState
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
