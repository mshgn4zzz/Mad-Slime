using System;

namespace MadSlime.Game.Logic.TimerStateMachine.States
{
    public class RunningTimerState : ITimerState
    {
        private readonly TimerData _data;

        public RunningTimerState(TimerData data)
        {
            _data = data;
        }

        public event Action<float> Ticked;
        public event Action Completed;

        public void Enter()
        {
            if (_data.Remaining <= 0f)
            {
                Completed?.Invoke();
            }
        }

        public void Exit()
        {
        }

        public void Tick(float delta)
        {
            if (delta <= 0f)
            {
                return;
            }

            _data.Consume(delta);
            Ticked?.Invoke(_data.Remaining);
        }

        public void CheckCompletion()
        {
            if (_data.Remaining <= 0f)
            {
                Completed?.Invoke();
            }
        }
    }
}
