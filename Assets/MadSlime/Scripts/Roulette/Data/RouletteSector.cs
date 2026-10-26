using System;
using UnityEngine;
namespace Roulette
{
    [Serializable]
    public class RouletteSector
    {
        [Tooltip("Награда сектора: монеты или скин.")]
        [SerializeField] private RewardType _rewardType = RewardType.Coins;

        [Tooltip("Количество монет (для типа Coins).")]
        [SerializeField, Min(0)] private int _coins = 100;

        [Tooltip("Шанс именно этого сектора в процентах. Сумма всех секторов должна быть 100. "
            + "Значения не перераспределяются автоматически. 0 — сектор не выпадает.")]
        [SerializeField, Range(0f, 100f)] private float _dropChance = 25f;

        public RewardType RewardType => _rewardType;
        public int Coins => _coins;
        public float DropChance => _dropChance;
    }
}
