using MadSlime.Player.Data;
using MadSlime.Player.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Player.View
{
    public class SpeedSmoke : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _smoke;

        [SerializeField] private PlayerScaler _playerScaler;

        private Vector3 _baseSmokeScale;
        private Vector3 _baseSmokePosition;
        private PlayerConfig _config;
        private Movement _movement;
        private bool _isSmoking;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_smoke, name, nameof(_smoke));

            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));

            ArgumentGuard.CheckAssigned(_movement, name, nameof(_movement));

            ArgumentGuard.CheckAssigned(_playerScaler, name, nameof(_playerScaler));

            _baseSmokeScale = _smoke.transform.localScale;
            _baseSmokePosition = _smoke.transform.localPosition;
            _smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void OnEnable()
        {
            _movement.SpeedChanged += OnSpeedChanged;
            _playerScaler.RelativeScaleChanged += OnScaleChanged;
            OnScaleChanged(_playerScaler.RelativeScale);

            Refresh();
        }

        private void OnDisable()
        {
            _movement.SpeedChanged -= OnSpeedChanged;
            _playerScaler.RelativeScaleChanged -= OnScaleChanged;

            SetSmoking(false);
        }

        [Inject]
        public void Construct(PlayerConfig config, Movement movement)
        {
            _config = config;
            _movement = movement;
        }

        private void Refresh()
        {
            float threshold = _config.SmokeSpeedThreshold;

            SetSmoking(threshold > 0f && _movement.CurrentSpeed > threshold);
        }

        private void SetSmoking(bool isSmoking)
        {
            if (_isSmoking == isSmoking)
            {
                return;
            }

            _isSmoking = isSmoking;

            if (isSmoking)
            {
                _smoke.Play(true);
            }
            else
            {
                _smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void OnScaleChanged(float multiplier)
        {
            _smoke.transform.localScale = _baseSmokeScale * multiplier;
            _smoke.transform.localPosition = _baseSmokePosition * multiplier;
        }

        private void OnSpeedChanged()
        {
            Refresh();
        }
    }
}
