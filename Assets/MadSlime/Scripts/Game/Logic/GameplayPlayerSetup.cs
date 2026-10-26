using CameraSystem;
using Cysharp.Threading.Tasks;
using Items;
using Player;
using Quota;
using System;
using System.Collections.Generic;
using System.Threading;
namespace Game
{
    public class GameplayPlayerSetup
    {
        private Movement _movement;
        private PlayerTier _playerTier;
        private PlayerScaler _playerScaler;
        private SmellAbilityActivator _smellAbility;
        private GrowthAnimator _growthAnimator;
        private CameraFollow _cameraFollow;

        public GameplayPlayerSetup(Movement movement, PlayerTier playerTier, PlayerScaler playerScaler,
            SmellAbilityActivator smellAbility, GrowthAnimator growthAnimator, CameraFollow cameraFollow)
        {
            if (movement == null)
            {
                throw new ArgumentNullException(nameof(movement));
            }

            if (playerTier == null)
            {
                throw new ArgumentNullException(nameof(playerTier));
            }

            if (playerScaler == null)
            {
                throw new ArgumentNullException(nameof(playerScaler));
            }

            if (smellAbility == null)
            {
                throw new ArgumentNullException(nameof(smellAbility));
            }

            if (growthAnimator == null)
            {
                throw new ArgumentNullException(nameof(growthAnimator));
            }

            if (cameraFollow == null)
            {
                throw new ArgumentNullException(nameof(cameraFollow));
            }

            _movement = movement;
            _playerTier = playerTier;
            _playerScaler = playerScaler;
            _smellAbility = smellAbility;
            _growthAnimator = growthAnimator;
            _cameraFollow = cameraFollow;
        }

        public void PrepareSession()
        {
            _playerTier.Initialize();
            _playerScaler.ApplyInitialScale();
            _movement.Initialize();
            _smellAbility.SetPaused(true);
        }

        public UniTask PrepareOutlineAsync(IReadOnlyList<Item> items, Action<float> onProgress,
            CancellationToken cancellationToken)
        {
            return _smellAbility.PrepareAsync(items, onProgress, cancellationToken);
        }

        public void StopSession()
        {
            _cameraFollow.StopMotion();
            _growthAnimator.Stop();
        }
    }
}
