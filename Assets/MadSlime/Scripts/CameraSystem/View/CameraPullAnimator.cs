using Collectables;
using Items;
using Player;
using System;
using UnityEngine;
using VContainer;
namespace CameraSystem
{
    public class CameraPullAnimator : CameraImpulseAnimator
    {
        [SerializeField] private CameraConfig _config;

        private ItemCollector _itemCollector;
        private PlayerTier _playerTier;
        private TierLookup _tierLookup;

        public float Pull => Impulse;

        [Inject]
        public void Construct(ItemCollector itemCollector, PlayerTier playerTier, TierTable tierTable)
        {
            _itemCollector = itemCollector;
            _playerTier = playerTier;
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
                        + $"registers ItemCollector and CameraPullAnimator.");
            }

            if (_playerTier == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerTier was not injected. Check that GameLifetimeScope "
                        + $"registers PlayerTier and CameraPullAnimator.");
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
            _playerTier.TierChanged += OnTierChanged;
        }

        private void OnDisable()
        {
            _itemCollector.ItemCollected -= OnItemCollected;
            _playerTier.TierChanged -= OnTierChanged;

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
            float strength = _config.MassToPullStrength.Evaluate(mass);
            float newPull = Mathf.Min(Pull + strength, _config.MaxPull);

            PlayRecoverFrom(newPull, 1f / _config.RecoverSpeed);
        }

        private void OnTierChanged(SizeTier previousTier, SizeTier currentTier)
        {
            if (currentTier <= previousTier)
            {
                return;
            }

            float newPull = Mathf.Max(Pull - _config.TierPushStrength, -_config.MaxPush);

            PlayRecoverFrom(newPull, 1f / _config.RecoverSpeed);
        }
    }
}
