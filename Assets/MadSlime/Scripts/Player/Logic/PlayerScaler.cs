using System;
using System.Collections.Generic;
using MadSlime.CameraSystem.Logic;
using MadSlime.Items.Data;
using MadSlime.Player.View;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Player.Logic
{
    public class PlayerScaler : MonoBehaviour
    {
        [SerializeField] private Transform _modelTransform;
        [SerializeField] private Transform _rootTransform;
        [SerializeField] private CapsuleCollider _playerCollider;
        [SerializeField] private CameraFollow _cameraFollow;
        [SerializeField] private GrowthAnimator _growthAnimator;
        [SerializeField] private List<RadiusRecipientEntry> _radiusRecipients = new List<RadiusRecipientEntry>();

        private Clamper _clamper;
        private float _baseClampRadius;
        private PlayerTier _playerTier;
        private TierResolver _tierResolver;

        private float _baseColliderRadius;
        private float _baseColliderHeight;
        private float _baseColliderCenterY;
        private float _currentMultiplier = 1f;
        private float _initialMultiplier = 1f;
        private float _relativeScale = 1f;

        public event Action<float> RelativeScaleChanged;

        public float RelativeScale => _relativeScale;

        public Vector3 WorldCenter => _playerCollider.bounds.center;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_playerTier, name, nameof(_playerTier));

            ArgumentGuard.CheckAssigned(_modelTransform, name, nameof(_modelTransform));

            ArgumentGuard.CheckAssigned(_rootTransform, name, nameof(_rootTransform));

            ArgumentGuard.CheckAssigned(_playerCollider, name, nameof(_playerCollider));

            ArgumentGuard.CheckAssigned(_cameraFollow, name, nameof(_cameraFollow));

            ArgumentGuard.CheckAssigned(_growthAnimator, name, nameof(_growthAnimator));

            for (int i = 0; i < _radiusRecipients.Count; i++)
            {
                _radiusRecipients[i].Initialize(name);
            }

            _baseClampRadius = _clamper.Radius;

            _baseColliderHeight = _playerCollider.height;
            _baseColliderRadius = _playerCollider.radius;
            _baseColliderCenterY = _playerCollider.center.y;
        }

        private void OnEnable()
        {
            _playerTier.TierChanged += OnTierChanged;
        }

        private void OnDisable()
        {
            _playerTier.TierChanged -= OnTierChanged;
        }

        [Inject]
        public void Construct(PlayerTier playerTier, TierResolver tierResolver, Clamper clamper)
        {
            ArgumentGuard.CheckNotNull(clamper, nameof(clamper));
            _clamper = clamper;
            _playerTier = playerTier;
            _tierResolver = tierResolver;
        }

        public void ApplyInitialScale()
        {
            SizeTier currentTier = _playerTier.CurrentTier;

            _cameraFollow.SetOffsetMultiplier(_tierResolver.GetCameraOffsetFor(currentTier));

            float targetMultiplier = _tierResolver.GetScaleFor(currentTier);

            _initialMultiplier = targetMultiplier;
            _currentMultiplier = targetMultiplier;
            ApplyMultiplier();
        }

        private void ApplyMultiplier()
        {
            _modelTransform.localScale = Vector3.one * _currentMultiplier;
            _modelTransform.localPosition = new Vector3(0f, _baseColliderCenterY * _currentMultiplier, 0f);

            _relativeScale = _currentMultiplier / _initialMultiplier;
            RelativeScaleChanged?.Invoke(_relativeScale);

            _playerCollider.height = _baseColliderHeight * _currentMultiplier;
            _playerCollider.radius = _baseColliderRadius * _currentMultiplier;
            _playerCollider.center = new Vector3(0f, _baseColliderCenterY * _currentMultiplier, 0f);

            _rootTransform.position =
                new Vector3(_rootTransform.position.x, _playerCollider.radius, _rootTransform.position.z);

            _clamper.SetRadius(_baseClampRadius * _currentMultiplier);

            for (int i = 0; i < _radiusRecipients.Count; i++)
            {
                RadiusRecipientEntry entry = _radiusRecipients[i];

                entry.Recipient.SetRadius(entry.BaseRadius * _currentMultiplier);
            }
        }

        private void OnTierChanged(SizeTier previousTier, SizeTier currentTier)
        {
            _cameraFollow.SetOffsetMultiplier(_tierResolver.GetCameraOffsetFor(currentTier));

            float targetMultiplier = _tierResolver.GetScaleFor(currentTier);

            _growthAnimator.Play(_currentMultiplier, targetMultiplier, OnGrowthValueChanged);
        }

        private void OnGrowthValueChanged(float multiplier)
        {
            _currentMultiplier = multiplier;

            ApplyMultiplier();
        }
    }
}
