using UnityEngine;

namespace Game
{
    [CreateAssetMenu(menuName = "Mad Slime/Yandex Config", fileName = "NewYandexConfig")]
    public class YandexConfig : ScriptableObject
    {
        [Header("Ads")]
        [SerializeField] private string _doubleRewardId = "DoubleReward";

        [SerializeField] private string _fillRescueRewardId = "NextLevel";

        [SerializeField] private string _rouletteRewardId = "Roulette";

        [Header("Leaderboard")]
        [SerializeField] private string _leaderboardName = "CollectedItems";

        public string DoubleRewardId => _doubleRewardId;

        public string FillRescueRewardId => _fillRescueRewardId;

        public string RouletteRewardId => _rouletteRewardId;

        public string LeaderboardName => _leaderboardName;
    }
}
