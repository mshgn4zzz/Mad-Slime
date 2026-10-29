using System;
using MadSlime.Game.Logic.TimerStateMachine.States;
using MadSlime.StateMachine.Logic;
using MadSlime.Validation;

namespace MadSlime.Game.Logic.TimerStateMachine
{
    public class TimerStateMachine : BaseStateMachine<ITimerState>, IDisposable
    {
        private readonly TimerData _data = new TimerData();
        private readonly ReadyTimerState _ready = new ReadyTimerState();
        private readonly CompletedTimerState _completed = new CompletedTimerState();
        private readonly RunningTimerState _running;

        public TimerStateMachine()
        {
            _running = new RunningTimerState(_data);
            RegisterState(_ready);
            RegisterState(_running);
            RegisterState(_completed);
            Subscribe();
        }

        public event Action<float> Ticked;
        public event Action Finished;

        public void Configure(float duration)
        {
            ArgumentGuard.CheckPositive(duration, nameof(duration));

            _data.Reset(duration);

            if (CurrentState != _running)
            {
                SwitchState<ReadyTimerState>();
            }
        }

        public void StartCount()
        {
            if (CurrentState == null)
            {
                throw new InvalidOperationException("Call Setup before StartCount.");
            }

            if (CurrentState == _running)
            {
                throw new InvalidOperationException("Timer is already running.");
            }

            SwitchState<RunningTimerState>();
        }

        public void StopCount()
        {
            if (CurrentState == _running)
            {
                SwitchState<ReadyTimerState>();
            }
        }

        public void Tick(float delta)
        {
            if (CurrentState != _running)
            {
                return;
            }

            _running.Tick(delta);

            if (CurrentState == _running)
            {
                _running.CheckCompletion();
            }
        }

        public void Dispose()
        {
            Unsubscribe();
            base.Stop();
        }

        private void Subscribe()
        {
            _running.Ticked += OnTicked;
            _running.Completed += OnCompleted;
            _completed.Entered += OnFinished;
        }

        private void Unsubscribe()
        {
            _running.Ticked -= OnTicked;
            _running.Completed -= OnCompleted;
            _completed.Entered -= OnFinished;
        }

        private void OnTicked(float remaining)
        {
            Ticked?.Invoke(remaining);
        }

        private void OnCompleted()
        {
            if (CurrentState != _running)
            {
                return;
            }

            _data.Complete();
            SwitchState<CompletedTimerState>();
        }

        private void OnFinished()
        {
            Finished?.Invoke();
        }
    }
}
