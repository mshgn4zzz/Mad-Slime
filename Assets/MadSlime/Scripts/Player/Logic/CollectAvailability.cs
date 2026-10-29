using MadSlime.Items.Data;
using MadSlime.Upgrades.Logic;
using MadSlime.Validation;

namespace MadSlime.Player.Logic
{
    public class CollectAvailability
    {
        private const int AmbitionTierOffset = 1;

        private readonly PlayerTier _playerTier;
        private readonly PlayerUpgrades _upgrades;

        public CollectAvailability(PlayerTier playerTier, PlayerUpgrades upgrades)
        {
            _playerTier = playerTier;
            _upgrades = upgrades;

            ArgumentGuard.CheckNotNull(_playerTier, nameof(playerTier));

            ArgumentGuard.CheckNotNull(_upgrades, nameof(upgrades));
        }

        public bool IsCollectable(SizeTier itemTier)
        {
            int allowedTier = (int)_playerTier.CurrentTier;

            if (_upgrades.IsAmbitionsUnlocked())
            {
                allowedTier += AmbitionTierOffset;
            }

            return (int)itemTier <= allowedTier;
        }
    }
}
