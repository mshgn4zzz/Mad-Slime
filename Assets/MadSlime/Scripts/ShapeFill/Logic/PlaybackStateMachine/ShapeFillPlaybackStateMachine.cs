using System;
using MadSlime.ShapeFill.Data;
using MadSlime.ShapeFill.Logic.PlaybackStateMachine.States;
using MadSlime.ShapeFill.View;
using MadSlime.StateMachine.Logic;
using MadSlime.Validation;

namespace MadSlime.ShapeFill.Logic.PlaybackStateMachine
{
    public class ShapeFillPlaybackStateMachine : BaseStateMachine<IShapeFillPlaybackState>, IDisposable
    {
        private readonly ShapeFillPlaybackData _data;
        private readonly FillPlaybackSpeed _normalSpeed;
        private readonly FillPlaybackSpeed _boostedSpeed;
        private readonly NormalFillState _normal = new NormalFillState();
        private readonly BoostedFillState _boosted = new BoostedFillState();
        private readonly InstantFillState _instant = new InstantFillState();
        private readonly AwaitingArrivalsState _awaiting = new AwaitingArrivalsState();
        private readonly CompletedPlaybackState _completed = new CompletedPlaybackState();

        public ShapeFillPlaybackStateMachine(ShapeFillPlaybackData data, FillConfig config)
        {
            _data = data;
            _normalSpeed = new FillPlaybackSpeed(
                config.SpawnInterval,
                config.FlightDuration,
                config.MaxFillDuration);
            _boostedSpeed = new FillPlaybackSpeed(
                config.BoostedSpawnInterval,
                config.BoostedFlightDuration,
                config.MaxBoostedFillDuration);
            RegisterState(_normal);
            RegisterState(_boosted);
            RegisterState(_instant);
            RegisterState(_awaiting);
            RegisterState(_completed);
            Subscribe();
        }

        public event Action PlaybackRequested;
        public event Action<FillPlaybackSpeed> SpeedChanged;
        public event Action InstantRequested;
        public event Action<float> Completed;
        public event Action<CubeFlightAnimator> CubeArrived;

        public void Reset()
        {
            Stop();
            _data.Reset(null);
        }

        public bool IsSpawning()
        {
            return CurrentState == _normal || CurrentState == _boosted || CurrentState == _instant;
        }

        public bool IsRescueAvailable()
        {
            return _data.Result != null && IsSpawning() == false && _data.Target < _data.Capacity;
        }

        public void ShowResult(FillResult result)
        {
            ArgumentGuard.CheckNotNull(result, nameof(result));
            Stop();
            _data.Reset(result);

            if (_data.Target <= 0)
            {
                SwitchState<CompletedPlaybackState>();
            }
            else
            {
                SwitchState<NormalFillState>();
                PlaybackRequested?.Invoke();
            }
        }

        public void Rescue()
        {
            if (IsRescueAvailable() == false)
            {
                throw new InvalidOperationException("Rescue requires a result below full capacity.");
            }

            _data.Target = _data.Capacity;
            SwitchState<NormalFillState>();
            PlaybackRequested?.Invoke();
        }

        public void Accelerate()
        {
            if (CurrentState == _normal)
            {
                SwitchState<BoostedFillState>();
            }
            else if (CurrentState == _boosted)
            {
                SwitchState<InstantFillState>();
            }
        }

        public void SpawningCompleted()
        {
            if (IsSpawning() == false)
            {
                return;
            }

            if (_data.Arrived >= _data.Target)
            {
                SwitchState<CompletedPlaybackState>();
            }
            else
            {
                SwitchState<AwaitingArrivalsState>();
            }
        }

        public void NotifyArrival(CubeFlightAnimator cube)
        {
            if (CurrentState == null || CurrentState == _completed)
            {
                return;
            }

            _data.Arrived++;
            CubeArrived?.Invoke(cube);

            if (CurrentState != null && CurrentState != _completed && _data.Arrived >= _data.Target)
            {
                SwitchState<CompletedPlaybackState>();
            }
        }

        public void Dispose()
        {
            Unsubscribe();
            base.Stop();
        }

        private void Subscribe()
        {
            _normal.Entered += OnNormal;
            _boosted.Entered += OnBoosted;
            _instant.Entered += OnInstant;
            _completed.Entered += OnCompleted;
        }

        private void Unsubscribe()
        {
            _normal.Entered -= OnNormal;
            _boosted.Entered -= OnBoosted;
            _instant.Entered -= OnInstant;
            _completed.Entered -= OnCompleted;
        }

        private void OnNormal()
        {
            SpeedChanged?.Invoke(_normalSpeed);
        }

        private void OnBoosted()
        {
            SpeedChanged?.Invoke(_boostedSpeed);
        }

        private void OnInstant()
        {
            InstantRequested?.Invoke();
        }

        private void OnCompleted()
        {
            float fraction = 0f;

            if (_data.Capacity > 0)
            {
                fraction = (float)_data.Target / _data.Capacity;
            }

            Completed?.Invoke(fraction);
        }
    }
}
