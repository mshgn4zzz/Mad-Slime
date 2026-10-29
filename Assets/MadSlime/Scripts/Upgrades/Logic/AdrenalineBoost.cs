using System;
using MadSlime.Player.Logic;
using MadSlime.Upgrades.Data;
using MadSlime.Validation;

namespace MadSlime.Upgrades.Logic
{
    public class AdrenalineBoost
    {
        private readonly PlayerUpgrades _upgrades;
        private readonly Movement _movement;
        private bool _isBoosted;

        public AdrenalineBoost(PlayerUpgrades upgrades, Movement movement)
        {
            _upgrades = upgrades;
            _movement = movement;

            ArgumentGuard.CheckNotNull(_upgrades, nameof(upgrades));

            ArgumentGuard.CheckNotNull(_movement, nameof(movement));
        }

        public event Action BoostStarted;

        public void Enable()
        {
            if (_isBoosted)
            {
                return;
            }

            if (_upgrades.IsAdrenalineUnlocked() == false)
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
        }
    }
}
