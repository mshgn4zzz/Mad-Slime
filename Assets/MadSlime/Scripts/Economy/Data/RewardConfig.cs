using System;
using UnityEngine;

namespace Economy
{
    [CreateAssetMenu(menuName = "Mad Slime/Reward Config", fileName = "NewRewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        [SerializeField, Min(0)] private int _baseReward = 20;

        [SerializeField, Min(1f)] private float _growPower = 1.65f;

        [SerializeField] private float _minPayFill = 0.25f;

        [SerializeField] private int _loseDivideBy = 4;

        public float LoseMultiplierThreshold => _minPayFill;

        public int LoseRewardDivisor => _loseDivideBy;

        public int GetReward(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            if (_baseReward < 0 || _growPower < 1f
                || float.IsNaN(_growPower) || float.IsInfinity(_growPower))
            {
                throw new InvalidOperationException($"{name}: reward progression settings are invalid.");
            }

            double reward = Math.Round(_baseReward * Math.Pow(level, _growPower), MidpointRounding.AwayFromZero);

            if (reward > int.MaxValue)
            {
                throw new OverflowException("The level reward exceeds the wallet's integer range.");
            }

            return (int)reward;
        }
    }
}
