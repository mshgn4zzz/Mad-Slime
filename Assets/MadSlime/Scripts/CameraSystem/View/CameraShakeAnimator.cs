using Collectables;
using Items;
using System;
using UnityEngine;
using VContainer;
namespace CameraSystem
{
    public class CameraShakeAnimator : CameraImpulseAnimator
    {
        [SerializeField] private CameraConfig _config;

        private ItemCollector _itemCollector;
        private TierLookup _tierLookup;

        public float Shake => Impulse;

        [Inject]
        public void Construct(ItemCollector itemCollector, TierTable tierTable)
        {
            _itemCollector = itemCollector;
            _tierLookup = new TierLookup(tierTable);
        }

        private void Awake()
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: CameraConfig is not assigned. Drag the CameraConfig asset into the _config field.");
            }

            if (_itemCollector == null)
            {
                throw new InvalidOperationException(
                    $"{name}: ItemCollector was not injected. Check that GameLifetimeScope "
                        + $"registers ItemCollector and CameraShakeAnimator.");
            }

            if (_tierLookup == null)
            {
                throw new InvalidOperationException(
                    $"{name}: TierTable was not injected. Check that ProjectLifetimeScope has "
                        + $"the TierTable asset assigned.");
            }
        }

        private void OnEnable()
        {
            _itemCollector.ItemCollected += OnItemCollected;
        }

        private void OnDisable()
        {
            _itemCollector.ItemCollected -= OnItemCollected;

            KillRecoverTween();
        }

        private void OnItemCollected(Item item)
        {
            if (item.Definition == null)
            {
                throw new InvalidOperationException(
                    $"{name}: collected item '{item.name}' has no Definition assigned. " +
                    "Assign a definition to its prefab or delete the item from the scene.");
            }

            float mass = _tierLookup.Get(item.Definition.Tier).Mass;

            if (mass < _config.ShakeMassThreshold)
            {
                return;
            }

            float newShake = Mathf.Min(Shake + _config.ShakeStrength, _config.MaxShake);
            float recoverDuration = 1f / _config.ShakeRecoverSpeed;

            PlayRecoverFrom(newShake, recoverDuration);
        }
    }
}
