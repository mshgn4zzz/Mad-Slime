using System;
using System.Collections.Generic;
using MadSlime.Roulette.Data;
using MadSlime.Skins.Data;
using MadSlime.Validation;

namespace MadSlime.Roulette.Logic
{
    public class RouletteConfigValidator
    {
        private const float FullChance = 100f;
        private const float ChanceTolerance = 0.01f;

        public void Validate(RouletteConfig config, IReadOnlyList<SkinItem> catalog, int skinSlotCount)
        {
            ArgumentGuard.CheckNotNull(config, nameof(config));
            ArgumentGuard.CheckAssigned(config.RarityTable, config.name, nameof(config.RarityTable));
            ArgumentGuard.CheckPositive(skinSlotCount, nameof(skinSlotCount));

            ValidatePrices(config);
            ValidateAccessTimers(config);
            ValidateMotion(config);
            ValidateDailySectors(config);
            ValidateSkinProbabilities(config, skinSlotCount);

            HashSet<string> catalogIds = ValidateCatalog(config, catalog);

            ValidateExclusiveSkins(config, catalogIds);
        }

        private void ValidatePrices(RouletteConfig config)
        {
            if (config.SkinSpinMinCost <= 0 || config.SkinSpinCostStep < 0
                || config.SkinSpinMaxCost < 0 || config.DuplicateCoinsCompensation <= 0
                || (config.SkinSpinMaxCost > 0 && config.SkinSpinMaxCost < config.SkinSpinMinCost))
            {
                throw new InvalidOperationException("Roulette prices and compensation are invalid.");
            }
        }

        private void ValidateAccessTimers(RouletteConfig config)
        {
            if (config.FreeSpinCooldownSeconds <= 0 || config.AdSpinWindowSeconds <= 0
                || config.AdSpinsPerWindow <= 0)
            {
                throw new InvalidOperationException("Roulette access timers must be positive.");
            }
        }

        private void ValidateMotion(RouletteConfig config)
        {
            if (config.MinTurns <= 0 || config.MaxTurns < config.MinTurns || config.SpinDuration <= 0f
                || config.WindBackDuration <= 0f || config.SpinEasePower < 1f || config.WinDwellSeconds < 0f
                || config.IdleRotationSpeed < 0f || config.WindBackDegrees < 0f
                || config.SettleMaxDeviationDegrees < 0f)
            {
                throw new InvalidOperationException("Roulette motion configuration is invalid.");
            }
        }

        private void ValidateDailySectors(RouletteConfig config)
        {
            float sectorWeight = 0f;
            int hiddenSectors = 0;

            for (int i = 0; i < config.Sectors.Count; i++)
            {
                RouletteSector sector = config.Sectors[i];

                if (sector == null || IsWeightValid(sector.DropChance) == false)
                {
                    throw new InvalidOperationException("Roulette sectors contain invalid weights.");
                }

                sectorWeight += sector.DropChance;

                if (sector.RewardType == RewardType.Coins && sector.Coins <= 0)
                {
                    throw new InvalidOperationException("Roulette coin sector must grant a positive amount.");
                }

                if (sector.RewardType == RewardType.Skin)
                {
                    hiddenSectors++;

                    ArgumentGuard.CheckNotEmpty(config.ExclusiveSkins, config.name, nameof(config.ExclusiveSkins));
                }
            }

            if (Math.Abs(sectorWeight - FullChance) > ChanceTolerance)
            {
                throw new InvalidOperationException($"{config.name}: daily sector chances must "
                    + $"total 100%, got {sectorWeight}%.");
            }

            if (hiddenSectors != 1)
            {
                throw new InvalidOperationException("Daily roulette requires exactly one hidden sector.");
            }
        }

        private void ValidateSkinProbabilities(RouletteConfig config, int skinSlotCount)
        {
            RouletteProbabilityRules probabilityRules = new RouletteProbabilityRules(config);
            SkinRarityLookup rarityLookup = new SkinRarityLookup(config.RarityTable);
            int showcaseSize = 0;
            float skinWeight = 0f;

            for (int rarityIndex = 0; rarityIndex <= (int)SkinRarity.Legendary; rarityIndex++)
            {
                SkinRarity rarity = (SkinRarity)rarityIndex;

                rarityLookup.Get(rarity);

                float weight = probabilityRules.GetSkinDropChance(rarity);
                int quota = probabilityRules.GetShowcaseQuota(rarity);

                if (IsWeightValid(weight) == false || quota < 0 || (quota > 0 && weight <= 0f))
                {
                    throw new InvalidOperationException("Skin roulette quotas and weights are invalid.");
                }

                showcaseSize += quota;
                skinWeight += weight;
            }

            if (Math.Abs(skinWeight - FullChance) > ChanceTolerance)
            {
                throw new InvalidOperationException($"{config.name}: skin rarity chances must total "
                    + $"100%, got {skinWeight}%.");
            }

            if (showcaseSize <= 0 || showcaseSize > skinSlotCount)
            {
                throw new InvalidOperationException("Skin showcase must fit the wheel.");
            }
        }

        private HashSet<string> ValidateCatalog(RouletteConfig config, IReadOnlyList<SkinItem> catalog)
        {
            ArgumentGuard.CheckNotEmpty(catalog, config.name, nameof(catalog));

            HashSet<string> ids = new HashSet<string>();

            for (int i = 0; i < catalog.Count; i++)
            {
                ValidateSkin(catalog[i]);

                if (ids.Add(catalog[i].Id) == false)
                {
                    throw new InvalidOperationException("Skin roulette catalog contains duplicate IDs.");
                }
            }

            return ids;
        }

        private void ValidateExclusiveSkins(RouletteConfig config, HashSet<string> ids)
        {
            HashSet<string> exclusiveIds = new HashSet<string>();

            for (int i = 0; i < config.ExclusiveSkins.Count; i++)
            {
                SkinItem skin = config.ExclusiveSkins[i];

                ValidateSkin(skin);

                if (ids.Contains(skin.Id) == false || exclusiveIds.Add(skin.Id) == false)
                {
                    throw new InvalidOperationException("Exclusive skins must be unique entries "
                        + "from the skin catalog.");
                }
            }
        }

        private bool IsWeightValid(float weight)
        {
            return weight >= 0f && weight <= FullChance
                && float.IsNaN(weight) == false && float.IsInfinity(weight) == false;
        }

        private void ValidateSkin(SkinItem skin)
        {
            if (skin == null || string.IsNullOrWhiteSpace(skin.Id) || skin.Icon == null)
            {
                throw new InvalidOperationException("Roulette skin requires an ID and icon.");
            }
        }
    }
}
