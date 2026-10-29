using System;
using MadSlime.Items.Data;
using MadSlime.PlayerInput.Logic;
using MadSlime.Upgrades.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Player.Logic
{
    public class Movement : MonoBehaviour
    {
        [SerializeField] private Mover _mover;
        [SerializeField] private Rotator _rotator;

        private Clamper _clamper;
        private PlayerTier _playerTier;
        private TierResolver _tierResolver;
        private PlayerUpgrades _upgrades;
        private PlayerInputReader _inputReader;
        private float _boostMultiplier = 1f;
        private bool _isControlEnabled;

        private float _currentSpeed;
        private float _speed;

        public event Action SpeedChanged;

        public float CurrentSpeed => _currentSpeed;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_mover, name, nameof(_mover));

            ArgumentGuard.CheckAssigned(_rotator, name, nameof(_rotator));

            ArgumentGuard.CheckAssigned(_playerTier, name, nameof(_playerTier));

            ArgumentGuard.CheckAssigned(_tierResolver, name, nameof(_tierResolver));

            ArgumentGuard.CheckAssigned(_upgrades, name, nameof(_upgrades));

            ArgumentGuard.CheckAssigned(_inputReader, name, nameof(_inputReader));

            ArgumentGuard.CheckNotNull(_clamper, nameof(_clamper));
            _mover.Setup(_clamper);
        }

        private void OnEnable()
        {
            _playerTier.TierChanged += OnTierChanged;
        }

        private void OnDisable()
        {
            _playerTier.TierChanged -= OnTierChanged;
        }

        private void Update()
        {
            if (_isControlEnabled == false)
            {
                return;
            }

            Vector3 moveDirection = ConvertToWorldDirection(_inputReader.MoveInput);

            _mover.Move(moveDirection, _speed);
            SetActualSpeed(_mover.Velocity.magnitude);
            _rotator.Rotate(moveDirection);
        }

        [Inject]
        public void Construct(
            Clamper clamper,
            PlayerTier playerTier,
            TierResolver tierResolver,
            PlayerUpgrades upgrades,
            PlayerInputReader inputReader)
        {
            _clamper = clamper;
            _playerTier = playerTier;
            _tierResolver = tierResolver;
            _upgrades = upgrades;
            _inputReader = inputReader;
        }

        public void Initialize()
        {
            ApplySpeed();
        }

        public void EnableControl()
        {
            _isControlEnabled = true;
        }

        public void DisableControl()
        {
            _isControlEnabled = false;
            _mover.Stop();
            SetActualSpeed(0f);
        }

        public void SetBoostMultiplier(float multiplier)
        {
            ArgumentGuard.CheckPositive(multiplier, nameof(multiplier));

            _boostMultiplier = multiplier;

            ApplySpeed();
        }

        private void ApplySpeed()
        {
            float baseSpeed = _tierResolver.GetSpeedFor(_playerTier.CurrentTier) * _upgrades.GetSpeedMultiplier();

            _speed = baseSpeed * _boostMultiplier;
        }

        private void SetActualSpeed(float speed)
        {
            if (Mathf.Approximately(_currentSpeed, speed))
            {
                return;
            }

            _currentSpeed = speed;
            SpeedChanged?.Invoke();
        }

        private Vector3 ConvertToWorldDirection(Vector2 input)
        {
            return new Vector3(input.x, 0f, input.y);
        }

        private void OnTierChanged(SizeTier previousTier, SizeTier currentTier)
        {
            ApplySpeed();
        }
    }
}
