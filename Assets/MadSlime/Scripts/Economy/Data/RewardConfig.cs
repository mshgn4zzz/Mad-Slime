using UnityEngine;

namespace MadSlime.Economy.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Reward Config", fileName = "NewRewardConfig")]
    public class RewardConfig : ScriptableObject
    {
        [SerializeField, Min(0)] private int _baseReward = 20;

        [SerializeField, Min(1f)] private float _growPower = 1.65f;

        [SerializeField] private float _minPayFill = 0.25f;

        [SerializeField] private int _loseDivideBy = 4;

        public int BaseReward => _baseReward;

        public float GrowPower => _growPower;

        public float LoseMultiplierThreshold => _minPayFill;

        public int LoseRewardDivisor => _loseDivideBy;
    }
}
