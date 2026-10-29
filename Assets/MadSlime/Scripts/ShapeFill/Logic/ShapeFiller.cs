using System;
using Cysharp.Threading.Tasks;
using MadSlime.ShapeFill.Logic.PlaybackStateMachine;
using MadSlime.ShapeFill.View;
using UnityEngine;
using VContainer;

namespace MadSlime.ShapeFill.Logic
{
    [RequireComponent(typeof(CubeSpawner))]
    public class ShapeFiller : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _ghostBackground;
        private ShapeFillView _view;
        private ShapeFillPlaybackStateMachine _machine;
        private ShapeFillPlaybackData _data;

        public event Action<float> FillCompleted;
        public event Action<CubeFlightAnimator> CubeArrived;

        private void Awake()
        {
            _view.Configure(_ghostBackground, this.GetCancellationTokenOnDestroy());
        }

        private void OnEnable()
        {
            _machine.PlaybackRequested += OnPlaybackRequested;
            _machine.SpeedChanged += OnSpeedChanged;
            _machine.InstantRequested += OnInstantRequested;
            _machine.Completed += OnCompleted;
            _machine.CubeArrived += OnCubeArrived;
            _view.SpawningCompleted += OnSpawningCompleted;
            _view.CubeArrived += OnViewCubeArrived;
        }

        private void OnDisable()
        {
            _machine.Stop();
            _view.Disable();
            _machine.PlaybackRequested -= OnPlaybackRequested;
            _machine.SpeedChanged -= OnSpeedChanged;
            _machine.InstantRequested -= OnInstantRequested;
            _machine.Completed -= OnCompleted;
            _machine.CubeArrived -= OnCubeArrived;
            _view.SpawningCompleted -= OnSpawningCompleted;
            _view.CubeArrived -= OnViewCubeArrived;
        }

        [Inject]
        public void Construct(ShapeFillView view, ShapeFillPlaybackStateMachine machine, ShapeFillPlaybackData data)
        {
            _view = view;
            _machine = machine;
            _data = data;
        }

        public void Initialize()
        {
            _view.Initialize();
        }

        public void BuildShape()
        {
            _machine.Reset();
            _view.BuildShape();
            _data.Capacity = _view.RequiredFillCount;
        }

        public void ShowResult(FillResult result)
        {
            _view.StopPlayback();
            _machine.ShowResult(result);
        }

        public void Rescue()
        {
            _machine.Rescue();
        }

        public void StopFill()
        {
            _machine.Stop();
            _view.StopPlayback();
        }

        public void Accelerate()
        {
            _machine.Accelerate();
        }

        public bool IsRescueAvailable()
        {
            return _machine.IsRescueAvailable();
        }

        public float GetFillFraction()
        {
            if (_data.Target <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01(_data.Arrived / (float)_data.Target);
        }

        public float GetFormFillFraction()
        {
            if (_data.Capacity <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01(_data.Arrived / (float)_data.Capacity);
        }

        private void OnPlaybackRequested()
        {
            _view.Play();
        }

        private void OnSpeedChanged(FillPlaybackSpeed speed)
        {
            _view.SetSpeed(speed);
        }

        private void OnInstantRequested()
        {
            _view.CompleteInstantly();
        }

        private void OnSpawningCompleted()
        {
            _machine.SpawningCompleted();
        }

        private void OnViewCubeArrived(CubeFlightAnimator cube)
        {
            _machine.NotifyArrival(cube);
        }

        private void OnCompleted(float fraction)
        {
            FillCompleted?.Invoke(fraction);
        }

        private void OnCubeArrived(CubeFlightAnimator cube)
        {
            CubeArrived?.Invoke(cube);
        }
    }
}
