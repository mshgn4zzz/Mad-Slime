using Player;
using System;
using UnityEngine;
using VContainer;
namespace Upgrades
{
    public class AdrenalineBoost : MonoBehaviour
    {
        private PlayerUpgrades _upgrades;
        private Movement _movement;
        private bool _isBoosted;

        public event Action BoostStarted;
        public event Action BoostEnded;

        [Inject]
        public void Construct(PlayerUpgrades upgrades, Movement movement)
        {
            _upgrades = upgrades;
            _movement = movement;
        }

        private void Awake()
        {
            if (_upgrades == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerUpgrades was not injected. Check that ProjectLifetimeScope "
                        + $"registers PlayerUpgrades.");
            }

            if (_movement == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Movement was not injected. Check that GameLifetimeScope registers "
                        + $"Movement and AdrenalineBoost.");
            }

        }

        public void Enable()
        {
            if (_isBoosted == true)
            {
                return;
            }

            if (_upgrades.HasAdrenaline() == false)
            {
                return;
            }

            _isBoosted = true;
            _movement.SetBoostMultiplier(_upgrades.GetPerk(PerkType.Adrenaline).Value);

            BoostStarted?.Invoke();
        }

        public void Disable()
        {
            if (_isBoosted == false)
            {
                return;
            }

            _isBoosted = false;
            _movement.SetBoostMultiplier(1f);

            BoostEnded?.Invoke();
        }
    }
}
