using System;
using System.Collections.Generic;
using MadSlime.Items.Data;
using MadSlime.Levels.Data;
using MadSlime.Quota.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MadSlime.Quota.Logic
{
    public class QuotaGenerator
    {
        private const SizeTier LargestQuotaTier = SizeTier.Giant;

        private List<ItemDefinition> _candidates = new List<ItemDefinition>();
        private Dictionary<SizeTier, int> _tierEntryCounts = new Dictionary<SizeTier, int>();
        private HashSet<ItemDefinition> _selected = new HashSet<ItemDefinition>();

        public List<QuotaEntry> Generate(IReadOnlyDictionary<ItemDefinition, int> spawnedCounts, LevelConfig config)
        {
            Validate(config);
            CollectCandidates(spawnedCounts);

            if (_candidates.Count < config.QuotaTypesMin)
            {
                throw new InvalidOperationException(
                    $"{config.name}: quota requires at least {config.QuotaTypesMin} types, " +
                    $"but the layout spawned only {_candidates.Count}. Increase layout variety or reduce the minimum.");
            }

            int maximumTypes = Mathf.Min(config.QuotaTypesMax, _candidates.Count);
            int typesTarget = Random.Range(config.QuotaTypesMin, maximumTypes + 1);

            ShuffleCandidates();
            _tierEntryCounts.Clear();
            _selected.Clear();

            List<QuotaEntry> entries = new List<QuotaEntry>(typesTarget);

            for (int i = 0; i < _candidates.Count && entries.Count < typesTarget; i++)
            {
                ItemDefinition definition = _candidates[i];

                _tierEntryCounts.TryGetValue(definition.Tier, out int tierCount);

                if (tierCount >= config.QuotaMaxSameTier)
                {
                    continue;
                }

                AddEntry(entries, definition, spawnedCounts[definition], config);
                _tierEntryCounts[definition.Tier] = tierCount + 1;
            }

            for (int i = 0; i < _candidates.Count && entries.Count < typesTarget; i++)
            {
                ItemDefinition definition = _candidates[i];

                if (_selected.Contains(definition) == false)
                {
                    AddEntry(entries, definition, spawnedCounts[definition], config);
                }
            }

            return entries;
        }

        private void AddEntry(List<QuotaEntry> entries, ItemDefinition definition, int spawned, LevelConfig config)
        {
            int target = Random.Range(config.QuotaTargetMin, config.QuotaTargetMax + 1);

            entries.Add(new QuotaEntry(definition, Mathf.Min(target, spawned)));
            _selected.Add(definition);
        }

        private void CollectCandidates(IReadOnlyDictionary<ItemDefinition, int> spawnedCounts)
        {
            _candidates.Clear();

            foreach (KeyValuePair<ItemDefinition, int> pair in spawnedCounts)
            {
                if (pair.Key != null && pair.Value > 0 && pair.Key.Tier <= LargestQuotaTier)
                {
                    _candidates.Add(pair.Key);
                }
            }
        }

        private void ShuffleCandidates()
        {
            for (int i = _candidates.Count - 1; i > 0; i--)
            {
                int swapIndex = Random.Range(0, i + 1);

                ItemDefinition candidate = _candidates[i];

                _candidates[i] = _candidates[swapIndex];
                _candidates[swapIndex] = candidate;
            }
        }

        private void Validate(LevelConfig config)
        {
            if (config.QuotaTypesMin <= 0 || config.QuotaTypesMax < config.QuotaTypesMin
                || config.QuotaTargetMin <= 0 || config.QuotaTargetMax < config.QuotaTargetMin
                || config.QuotaMaxSameTier <= 0)
            {
                throw new InvalidOperationException($"{config.name}: quota ranges and preferred "
                    + $"tier limit must be positive and ordered.");
            }
        }
    }
}
