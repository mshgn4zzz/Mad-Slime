using System;
using UnityEngine;

namespace Roulette
{
    [Serializable]
    public class RouletteSector
    {
        [SerializeField] private RewardType _rewardType = RewardType.Coins;

        [SerializeField, Min(0)] private int _coins = 100;

        [SerializeField, Range(0f, 100f)] private float _dropChance = 25f;

        public RewardType RewardType => _rewardType;

        public int Coins => _coins;

        public float DropChance => _dropChance;
    }
}
