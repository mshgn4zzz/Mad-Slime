using System;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Mad Slime/Reward Config", fileName = "NewRewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        [Tooltip("Базовая награда монетами за победу на уровне.")]
        [SerializeField, Min(0)] private int _baseReward = 50;

        [Tooltip("Множитель награды за каждый следующий уровень.")]
        [SerializeField, Min(1f)] private float _rewardMultiplier = 1.27f;

        [Tooltip("Максимальная награда за победу до рекламного удвоения.")]
        [SerializeField, Min(0)] private int _maxReward = 5000;

        [Tooltip("Минимальная доля заливки при провале, за которую хоть что-то платят. Ниже — ноль монет.")]
        [SerializeField] private float _loseMultiplierThreshold = 0.25f;

        [Tooltip("На сколько делится базовая награда при провале. 4 = четверть базовой.")]
        [SerializeField] private int _loseRewardDivisor = 4;

        public float LoseMultiplierThreshold => _loseMultiplierThreshold;
        public int LoseRewardDivisor => _loseRewardDivisor;

        public int GetReward(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            if (_baseReward < 0 || _maxReward < _baseReward || _rewardMultiplier < 1f
                || float.IsNaN(_rewardMultiplier) || float.IsInfinity(_rewardMultiplier))
            {
                throw new InvalidOperationException($"{name}: reward progression settings are invalid.");
            }

            double reward = _baseReward * Math.Pow(_rewardMultiplier, level - 1);
            double cappedReward = Math.Min(reward, _maxReward);

            return (int)Math.Round(cappedReward, MidpointRounding.AwayFromZero);
        }
    }
}
