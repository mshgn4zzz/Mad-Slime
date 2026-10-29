using System;
using System.Collections.Generic;
using MadSlime.Items.Data;
using MadSlime.Player.Data;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Player.Logic
{
    public class TierResolver
    {
        private readonly PlayerConfig _config;

        private List<PlayerTierThreshold> _sortedByMass = new List<PlayerTierThreshold>();

        public TierResolver(PlayerConfig config)
        {
            _config = config;

            ArgumentGuard.CheckNotNull(_config, nameof(config));

            ArgumentGuard.CheckNotEmpty(_config.Thresholds, _config.name, nameof(_config.Thresholds));

            foreach (PlayerTierThreshold threshold in _config.Thresholds)
            {
                ArgumentGuard.CheckAssigned(threshold, _config.name, nameof(_config.Thresholds));
                ArgumentGuard.CheckPositive(threshold.ScaleMultiplier, _config.name, nameof(threshold.ScaleMultiplier));
            }

            _sortedByMass.AddRange(_config.Thresholds);
            _sortedByMass.Sort((left, right) => left.RequiredMass.CompareTo(right.RequiredMass));
        }

        public SizeTier GetUnlockedTier(int mass)
        {
            SizeTier unlocked = SizeTier.Small;

            for (int i = 0; i < _sortedByMass.Count; i++)
            {
                if (mass >= _sortedByMass[i].RequiredMass)
                {
                    unlocked = _sortedByMass[i].Tier;
                }
                else
                {
                    break;
                }
            }

            return unlocked;
        }

        public float GetSpeedFor(SizeTier tier)
        {
            PlayerTierThreshold threshold = GetThreshold(tier);

            return threshold.Speed;
        }

        public float GetTierProgress(int mass)
        {
            float previousThresholdMass = 0f;
            float nextThresholdMass = -1f;

            for (int i = 0; i < _sortedByMass.Count; i++)
            {
                int thresholdMass = _sortedByMass[i].RequiredMass;

                if (thresholdMass <= mass)
                {
                    previousThresholdMass = thresholdMass;
                }
                else
                {
                    nextThresholdMass = thresholdMass;
                    break;
                }
            }

            if (nextThresholdMass < 0f)
            {
                return 1f;
            }

            float segment = nextThresholdMass - previousThresholdMass;

            if (segment <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01((mass - previousThresholdMass) / segment);
        }

        public float GetScaleFor(SizeTier tier)
        {
            PlayerTierThreshold threshold = GetThreshold(tier);

            return threshold.ScaleMultiplier;
        }

        public float GetCameraOffsetFor(SizeTier tier)
        {
            PlayerTierThreshold threshold = GetThreshold(tier);

            return threshold.CameraOffsetMultiplier;
        }

        private PlayerTierThreshold GetThreshold(SizeTier tier)
        {
            for (int i = 0; i < _sortedByMass.Count; i++)
            {
                if (_sortedByMass[i].Tier == tier)
                {
                    ArgumentGuard.CheckPositive(
                        _sortedByMass[i].ScaleMultiplier, _config.name, nameof(PlayerTierThreshold.ScaleMultiplier));
                    return _sortedByMass[i];
                }
            }

            throw new InvalidOperationException(
                $"TierResolver: PlayerConfig '{_config.name}' has no threshold for tier '{tier}'. "
                + "Add the threshold row.");
        }
    }
}
