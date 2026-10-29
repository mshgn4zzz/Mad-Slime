using System;
using MadSlime.Items.Data;
using MadSlime.Player.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Player.View
{
    public class TierUpFx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _burst;

        private PlayerTier _playerTier;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_playerTier, name, nameof(_playerTier));

            ArgumentGuard.CheckAssigned(_burst, name, nameof(_burst));

            if (_burst.main.loop)
            {
                throw new InvalidOperationException(
                    $"{name}: the assigned ParticleSystem is looping. The tier-up burst must be "
                    + $"one-shot: uncheck Loop.");
            }

            _burst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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

            _burst.Play(true);
        }
    }
}
