using MadSlime.CameraSystem.Data;
using MadSlime.Items.Data;
using MadSlime.Player.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.CameraSystem.View
{
    [RequireComponent(typeof(Camera))]
    public class CameraFovKickAnimator : CameraImpulseAnimator
    {
        [SerializeField] private CameraConfig _config;

        private PlayerTier _playerTier;
        private Camera _camera;

        public float FovKick => Impulse;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.fieldOfView = _config.BaseFov;

            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));

            ArgumentGuard.CheckAssigned(_playerTier, name, nameof(_playerTier));
        }

        private void OnEnable()
        {
            _playerTier.TierChanged += OnTierChanged;
        }

        private void OnDisable()
        {
            _playerTier.TierChanged -= OnTierChanged;

            KillRecoverTween();
            _camera.fieldOfView = _config.BaseFov;
        }

        private void LateUpdate()
        {
            if (Time.deltaTime > 0f)
            {
                _camera.fieldOfView = _config.BaseFov + FovKick;
            }
        }

        [Inject]
        public void Construct(PlayerTier playerTier)
        {
            _playerTier = playerTier;
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
