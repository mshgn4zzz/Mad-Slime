using System;
using System.Collections.Generic;
using MadSlime.Items.Data;
using MadSlime.Items.View;
using MadSlime.Levels.Data;
using MadSlime.Validation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MadSlime.Levels.Logic
{
    public class PropVariantsAssigner
    {
        private Dictionary<Item, List<ItemDefinition>> _variantsByPrefab =
            new Dictionary<Item, List<ItemDefinition>>();

        private Dictionary<Item, ItemDefinition> _assignedByPrefab =
            new Dictionary<Item, ItemDefinition>();

        public Dictionary<Item, ItemDefinition> Assign(LevelConfig config)
        {
            _variantsByPrefab.Clear();
            _assignedByPrefab.Clear();

            ArgumentGuard.CheckAssigned(config.PropSet, config.name, nameof(config.PropSet));

            IReadOnlyList<PropVariant> variants = config.PropSet.Variants;

            if (variants.Count == 0)
            {
                throw new InvalidOperationException(
                    $"PropVariantsAssigner: PropSet '{config.PropSet.name}' has no baked "
                    + $"variants. Run Mad Slime → Prop Factory.");
            }

            CollectVariantsInRange(variants, config);

            if (_variantsByPrefab.Count == 0)
            {
                throw new InvalidOperationException(
                    $"PropVariantsAssigner: PropSet '{config.PropSet.name}' has no variants within tier range " +
                    $"{config.MinTier}-{config.MaxTier}. Re-run Prop Factory.");
            }

            List<Item> prefabs = new List<Item>(_variantsByPrefab.Keys);

            Shuffle(prefabs);

            List<SizeTier> tiers = GetTiersInRange(config);
            int guaranteedCount = Mathf.Min(tiers.Count, prefabs.Count);

            for (int i = 0; i < prefabs.Count; i++)
            {
                List<ItemDefinition> options = _variantsByPrefab[prefabs[i]];
                ItemDefinition chosen;

                if (i < guaranteedCount)
                {
                    chosen = FindByTier(options, tiers[i]);
                }
                else
                {
                    chosen = options[Random.Range(0, options.Count)];
                }

                _assignedByPrefab[prefabs[i]] = chosen;
            }

            return _assignedByPrefab;
        }

        private void CollectVariantsInRange(IReadOnlyList<PropVariant> variants, LevelConfig config)
        {
            foreach (PropVariant variant in variants)
            {
                if (variant == null || variant.Prefab == null || variant.Definition == null)
                {
                    throw new InvalidOperationException(
                        $"PropVariantsAssigner: PropSet '{config.PropSet.name}' contains an empty "
                        + $"variant. Re-run Mad Slime → Prop Factory.");
                }

                if (variant.Definition.Tier < config.MinTier || variant.Definition.Tier > config.MaxTier)
                {
                    continue;
                }

                if (_variantsByPrefab.TryGetValue(variant.Prefab, out List<ItemDefinition> list) == false)
                {
                    list = new List<ItemDefinition>();
                    _variantsByPrefab.Add(variant.Prefab, list);
                }

                list.Add(variant.Definition);
            }
        }

        private void Shuffle(List<Item> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);

                (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
            }
        }

        private List<SizeTier> GetTiersInRange(LevelConfig config)
        {
            List<SizeTier> tiers = new List<SizeTier>();

            for (int value = (int)config.MinTier; value <= (int)config.MaxTier; value++)
            {
                tiers.Add((SizeTier)value);
            }

            return tiers;
        }

        private ItemDefinition FindByTier(List<ItemDefinition> options, SizeTier tier)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].Tier == tier)
                {
                    return options[i];
                }
            }

            return options[0];
        }
    }
}
