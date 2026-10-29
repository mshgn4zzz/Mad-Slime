using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.CameraSystem.Logic;
using MadSlime.CameraSystem.View;
using MadSlime.Items.View;
using MadSlime.Player.Logic;
using MadSlime.Player.View;
using MadSlime.Quota.Logic;
using MadSlime.Validation;

namespace MadSlime.Game.Logic
{
    public class GameplayPlayerSetup
    {
        private Movement _movement;
        private PlayerTier _playerTier;
        private PlayerScaler _playerScaler;
        private SmellAbilityActivator _smellAbility;
        private GrowthAnimator _growthAnimator;
        private CameraFollow _cameraFollow;
        private CameraShakeAnimator _cameraShakeAnimator;
        private CameraFovKickAnimator _cameraFovKickAnimator;

        public GameplayPlayerSetup(
            Movement movement,
            PlayerTier playerTier,
            PlayerScaler playerScaler,
            SmellAbilityActivator smellAbility,
            GrowthAnimator growthAnimator,
            CameraFollow cameraFollow,
            CameraShakeAnimator cameraShakeAnimator,
            CameraFovKickAnimator cameraFovKickAnimator)
        {
            ArgumentGuard.CheckNotNull(movement, nameof(movement));

            ArgumentGuard.CheckNotNull(playerTier, nameof(playerTier));

            ArgumentGuard.CheckNotNull(playerScaler, nameof(playerScaler));

            ArgumentGuard.CheckNotNull(smellAbility, nameof(smellAbility));

            ArgumentGuard.CheckNotNull(growthAnimator, nameof(growthAnimator));

            ArgumentGuard.CheckNotNull(cameraFollow, nameof(cameraFollow));

            _movement = movement;
            _playerTier = playerTier;
            _playerScaler = playerScaler;
            _smellAbility = smellAbility;
            _growthAnimator = growthAnimator;
            _cameraFollow = cameraFollow;
            _cameraShakeAnimator = cameraShakeAnimator;
            _cameraFovKickAnimator = cameraFovKickAnimator;
        }

        public void PrepareSession()
        {
            _playerTier.Initialize();
            _playerScaler.ApplyInitialScale();
            _movement.Initialize();
            _smellAbility.SetPaused(true);
        }

        public UniTask PrepareOutlineAsync(
            IReadOnlyList<Item> items,
            Action<float> onProgress,
            CancellationToken cancellationToken)
        {
            return _smellAbility.PrepareAsync(items, onProgress, cancellationToken);
        }

        public void StopSession()
        {
            _cameraFollow.StopMotion();
            _cameraShakeAnimator.enabled = false;
            _cameraFovKickAnimator.enabled = false;
            _growthAnimator.Stop();
        }
    }
}
