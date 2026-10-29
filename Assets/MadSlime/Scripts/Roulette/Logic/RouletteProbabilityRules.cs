using MadSlime.Roulette.Data;
using MadSlime.Skins.Data;
using MadSlime.Validation;

namespace MadSlime.Roulette.Logic
{
    public class RouletteProbabilityRules
    {
        private RouletteConfig _config;

        public RouletteProbabilityRules(RouletteConfig config)
        {
            ArgumentGuard.CheckNotNull(config, nameof(config));

            _config = config;
        }

        public float GetSkinDropChance(SkinRarity rarity)
        {
            switch (rarity)
            {
                case SkinRarity.Rare:
                    return _config.RareDropChance;

                case SkinRarity.Epic:
                    return _config.EpicDropChance;

                case SkinRarity.Legendary:
                    return _config.LegendaryDropChance;

                default:
                    return _config.CommonDropChance;
            }
        }

        public int GetShowcaseQuota(SkinRarity rarity)
        {
            switch (rarity)
            {
                case SkinRarity.Rare:
                    return _config.ShowcaseRareCount;

                case SkinRarity.Epic:
                    return _config.ShowcaseEpicCount;

                case SkinRarity.Legendary:
                    return _config.ShowcaseLegendaryCount;

                default:
                    return 0;
            }
        }
    }
}
