using System;
using System.Collections.Generic;
using Skins;

namespace Roulette
{
    public class RouletteConfigValidator
    {
        private const float FullChance = 100f;
        private const float ChanceTolerance = 0.01f;

        public void Validate(RouletteConfig config, IReadOnlyList<SkinItem> catalog, int dailySlotCount, int skinSlotCount)
        {
            if (config == null || config.RarityTable == null)
            {
                throw new InvalidOperationException("Roulette config and rarity table must be assigned.");
            }

            if (config.Sectors.Count != dailySlotCount || skinSlotCount <= 0)
            {
                throw new InvalidOperationException("Configure exactly one daily reward per wheel sector.");
            }

            if (config.SkinSpinMinCost <= 0 || config.SkinSpinCostStep < 0
                || config.SkinSpinMaxCost < 0 || config.DuplicateCoinsCompensation <= 0
                || config.SkinSpinMaxCost > 0 && config.SkinSpinMaxCost < config.SkinSpinMinCost)
            {
                throw new InvalidOperationException("Roulette prices and compensation are invalid.");
            }

            if (config.FreeSpinCooldownSeconds <= 0 || config.AdSpinWindowSeconds <= 0 || config.AdSpinsPerWindow <= 0)
            {
                throw new InvalidOperationException("Roulette access timers must be positive.");
            }

            if (config.MinTurns <= 0 || config.MaxTurns < config.MinTurns || config.SpinDuration <= 0f
                || config.WindBackDuration <= 0f || config.SpinEasePower < 1f || config.WinDwellSeconds < 0f
                || config.IdleRotationSpeed < 0f || config.WindBackDegrees < 0f || config.SettleMaxDeviationDegrees < 0f)
            {
                throw new InvalidOperationException("Roulette motion configuration is invalid.");
            }

            if (config.LegendaryDropChance != 0f || config.ShowcaseLegendaryCount != 0)
            {
                throw new InvalidOperationException("Legendary skins are daily exclusives and cannot appear in paid roulette.");
            }

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

                if (sector.RewardType == RouletteSector.RewardKind.Coins && sector.Coins <= 0)
                {
                    throw new InvalidOperationException("Roulette coin sector must grant a positive amount.");
                }

                if (sector.RewardType == RouletteSector.RewardKind.Skin)
                {
                    hiddenSectors++;

                    if (config.ExclusiveSkins.Count == 0)
                    {
                        throw new InvalidOperationException("Hidden roulette sector requires exclusive skins.");
                    }
                }
            }

            if (hiddenSectors != 1 || Math.Abs(sectorWeight - FullChance) > ChanceTolerance)
            {
                throw new InvalidOperationException("Daily roulette requires exactly one hidden sector and a total chance of 100%.");
            }

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

                if (IsWeightValid(weight) == false || quota < 0 || quota > 0 && weight <= 0f)
                {
                    throw new InvalidOperationException("Skin roulette quotas and weights are invalid.");
                }

                showcaseSize += quota;
                skinWeight += weight;
            }

            if (showcaseSize <= 0 || showcaseSize > skinSlotCount || Math.Abs(skinWeight - FullChance) > ChanceTolerance)
            {
                throw new InvalidOperationException("Skin showcase must fit the wheel; rarity chances must total 100%.");
            }

            if (catalog == null || catalog.Count == 0)
            {
                throw new InvalidOperationException("Skin roulette requires a skin catalog.");
            }

            HashSet<string> ids = new HashSet<string>();

            for (int i = 0; i < catalog.Count; i++)
            {
                ValidateSkin(catalog[i]);

                if (ids.Add(catalog[i].Id) == false)
                {
                    throw new InvalidOperationException("Skin roulette catalog contains duplicate IDs.");
                }
            }

            HashSet<string> exclusiveIds = new HashSet<string>();

            for (int i = 0; i < config.ExclusiveSkins.Count; i++)
            {
                SkinItem skin = config.ExclusiveSkins[i];
                ValidateSkin(skin);

                if (skin.Rarity != SkinRarity.Legendary)
                {
                    throw new InvalidOperationException("Daily exclusive skins must be Legendary.");
                }

                if (ids.Contains(skin.Id) == false || exclusiveIds.Add(skin.Id) == false)
                {
                    throw new InvalidOperationException("Exclusive skins must be unique entries from the skin catalog.");
                }
            }

            for (int i = 0; i < catalog.Count; i++)
            {
                if (catalog[i].Rarity == SkinRarity.Legendary && exclusiveIds.Contains(catalog[i].Id) == false)
                {
                    throw new InvalidOperationException("Every Legendary skin must belong to the daily exclusive pool.");
                }
            }
        }

        private bool IsWeightValid(float weight)
        {
            return weight >= 0f && weight <= FullChance && float.IsNaN(weight) == false && float.IsInfinity(weight) == false;
        }

        private void ValidateSkin(SkinItem skin)
        {
            if (skin == null || string.IsNullOrWhiteSpace(skin.Id) == true || skin.Icon == null)
            {
                throw new InvalidOperationException("Roulette skin requires an ID and icon.");
            }
        }
    }
}
