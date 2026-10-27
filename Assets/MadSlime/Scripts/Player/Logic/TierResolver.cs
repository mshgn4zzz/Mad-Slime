using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Player
{
    public class TierResolver : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;

        private List<PlayerTierThreshold> _sortedByMass = new List<PlayerTierThreshold>();

        private void Awake()
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerConfig is not assigned. Drag the PlayerConfig asset into the _config field.");
            }

            if (_config.Thresholds.Count == 0)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerConfig '{_config.name}' has no tier thresholds. Add at "
                        + $"least one threshold row to the asset.");
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
                    ValidateScale(_sortedByMass[i], tier);
                    return _sortedByMass[i];
                }
            }

            throw new InvalidOperationException(
                $"{name}: PlayerConfig '{_config.name}' has no threshold for tier '{tier}'. Add the threshold row.");
        }

        private void ValidateScale(PlayerTierThreshold threshold, SizeTier tier)
        {
            if (threshold.ScaleMultiplier <= 0f)
            {
                throw new InvalidOperationException(
                    $"{name}: threshold for tier '{tier}' in PlayerConfig '{_config.name}' has ScaleMultiplier "
                        + $"{threshold.ScaleMultiplier}. It must be greater than zero, "
                        + $"otherwise the slime would collapse.");
            }
        }
    }
}
