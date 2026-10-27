using System;
using Items;
using Player;
using UnityEngine;
using VContainer;

namespace CameraSystem
{
    public class CameraFovKickAnimator : CameraImpulseAnimator
    {
        [SerializeField] private CameraConfig _config;

        private PlayerTier _playerTier;

        public float FovKick => Impulse;

        [Inject]
        public void Construct(PlayerTier playerTier)
        {
            _playerTier = playerTier;
        }

        private void Awake()
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: CameraConfig is not assigned. Drag the CameraConfig asset into the _config field.");
            }

            if (_playerTier == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerTier was not injected. Check that GameLifetimeScope "
                        + $"registers PlayerTier and CameraFovKickAnimator.");
            }
        }

        private void OnEnable()
        {
            _playerTier.TierChanged += OnTierChanged;
        }

        private void OnDisable()
        {
            _playerTier.TierChanged -= OnTierChanged;

            KillRecoverTween();
        }

        private void OnTierChanged(SizeTier previousTier, SizeTier currentTier)
        {
            if (currentTier <= previousTier)
            {
                return;
            }

            PlayRecoverFrom(_config.TierFovKick, 1f / _config.FovRecoverSpeed);
        }
    }
}
