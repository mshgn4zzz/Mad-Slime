using Items;
using PlayerInput;
using System;
using UnityEngine;
using Upgrades;
using VContainer;
namespace Player
{
    public class Movement : MonoBehaviour
    {
        private static Vector3 s_forward = new Vector3(0f, 0f, 1f);
        private static Vector3 s_right = new Vector3(1f, 0f, 0f);

        [SerializeField] private Mover _mover;
        [SerializeField] private Rotator _rotator;

        private PlayerTier _playerTier;
        private TierResolver _tierResolver;
        private PlayerUpgrades _upgrades;
        private PlayerInputReader _inputReader;
        private float _boostMultiplier = 1f;
        private bool _isControlEnabled;

        public event Action SpeedChanged;

        private float _currentSpeed;
        private float _movementSpeed;

        public float CurrentSpeed => _currentSpeed;

        [Inject]
        public void Construct(PlayerTier playerTier, TierResolver tierResolver, PlayerUpgrades upgrades,
            PlayerInputReader inputReader)
        {
            _playerTier = playerTier;
            _tierResolver = tierResolver;
            _upgrades = upgrades;
            _inputReader = inputReader;
        }

        private void Awake()
        {
            if (_mover == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Mover is not assigned. Drag the Mover component into the _mover field.");
            }

            if (_rotator == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Rotator is not assigned. Drag the Rotator component into the _rotator field.");
            }

            if (_playerTier == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerTier was not injected. Check that GameLifetimeScope "
                        + $"registers PlayerTier and Movement.");
            }

            if (_tierResolver == null)
            {
                throw new InvalidOperationException(
                    $"{name}: TierResolver was not injected. Check that GameLifetimeScope "
                        + $"registers TierResolver and Movement.");
            }

            if (_upgrades == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerUpgrades was not injected. Check that GameLifetimeScope "
                        + $"registers PlayerUpgrades and Movement.");
            }

            if (_inputReader == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerInputReader was not injected. Check that GameLifetimeScope "
                        + $"registers PlayerInputReader and Movement.");
            }

            if (TryGetComponent(out Clamper clamper) == false)
            {
                throw new InvalidOperationException(
                    $"{name}: Clamper is missing. Add a Clamper component to the player root.");
            }

            _mover.Setup(clamper);
        }

        public void Initialize()
        {
            ApplySpeed();
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

            _mover.Move(moveDirection, _movementSpeed);
            SetActualSpeed(_mover.Velocity.magnitude);
            _rotator.Rotate(moveDirection);
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
            if (multiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier),
                    "Movement.SetBoostMultiplier requires a positive multiplier.");
            }

            _boostMultiplier = multiplier;

            ApplySpeed();
        }

        private void OnTierChanged(SizeTier previousTier, SizeTier currentTier)
        {
            ApplySpeed();
        }

        private void ApplySpeed()
        {
            float baseSpeed = _tierResolver.GetSpeedFor(_playerTier.CurrentTier) * _upgrades.GetSpeedMultiplier();

            _movementSpeed = baseSpeed * _boostMultiplier;

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
            return s_forward * input.y + s_right * input.x;
        }
    }
}
