using System;
using MadSlime.Items.Data;
using MadSlime.Player.Data;
using MadSlime.Validation;

namespace MadSlime.Player.Logic
{
    public class PlayerTier
    {
        private readonly PlayerConfig _config;
        private readonly TierResolver _tierResolver;
        private int _mass;
        private SizeTier _currentTier = SizeTier.Small;

        public PlayerTier(PlayerConfig config, TierResolver tierResolver)
        {
            _config = config;
            _tierResolver = tierResolver;

            ArgumentGuard.CheckNotNull(_tierResolver, nameof(tierResolver));

            ArgumentGuard.CheckNotNull(_config, nameof(config));
        }

        public event Action<SizeTier, SizeTier> TierChanged;

        public event Action<int, int> MassChanged;

        public int Mass => _mass;

        public SizeTier CurrentTier => _currentTier;

        public void Initialize()
        {
            _mass = _config.StartMass;
            _currentTier = _tierResolver.GetUnlockedTier(_mass);
        }

        public void Add(int amount)
        {
            ArgumentGuard.CheckNonNegative(amount, nameof(amount));

            int previous = _mass;

            _mass += amount;
            MassChanged?.Invoke(previous, _mass);

            SizeTier previousTier = CurrentTier;
            SizeTier currentTier = _tierResolver.GetUnlockedTier(_mass);

            if (currentTier == previousTier)
            {
                return;
            }

            _currentTier = currentTier;
            TierChanged?.Invoke(previousTier, currentTier);
        }
    }
}
