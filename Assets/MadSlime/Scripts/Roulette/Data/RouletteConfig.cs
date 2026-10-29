using System.Collections.Generic;
using MadSlime.Audio.Data;
using MadSlime.Skins.Data;
using UnityEngine;

namespace MadSlime.Roulette.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Roulette Config", fileName = "NewRouletteConfig")]
    public class RouletteConfig : ScriptableObject
    {
        [Header("Daily Roulette")]
        [SerializeField] private List<RouletteSector> _sectors = new List<RouletteSector>();

        [SerializeField] private List<SkinItem> _rouletteSkins = new List<SkinItem>();

        [Header("Skin Roulette")]

        [SerializeField, Range(0f, 100f)] private float _commonDropChance = 78f;

        [SerializeField, Range(0f, 100f)] private float _rareDropChance = 17f;

        [SerializeField, Range(0f, 100f)] private float _epicDropChance = 3f;

        [SerializeField, Range(0f, 100f)] private float _legendaryDropChance = 2f;

        [SerializeField, Min(0)] private int _skinSpinMinCost = 300;

        [SerializeField, Min(0)] private int _skinSpinMaxCost = 0;

        [SerializeField, Min(0)] private int _skinSpinCostStep = 150;

        [SerializeField, Min(0)] private int _duplicateCoins = 100;

        [Header("Skin Showcase")]
        [SerializeField, Min(0)] private int _showcaseLegendaryCount = 1;

        [SerializeField, Min(0)] private int _showcaseEpicCount = 2;

        [SerializeField, Min(0)] private int _showcaseRareCount = 5;

        [Header("Rarity")]
        [SerializeField] private SkinRarityTable _rarityTable;

        [Header("Timers")]
        [SerializeField, Min(60)] private int _adSpinWindowSeconds = 1800;

        [SerializeField, Min(1)] private int _adSpinsPerWindow = 3;

        [SerializeField, Min(60)] private int _freeSpinCooldownSeconds = 1800;

        [Header("Wheel Motion")]
        [SerializeField, Min(0f)] private float _idleRotationSpeed = 12f;

        [SerializeField, Min(0f)] private float _pullBackDegrees = 25f;

        [SerializeField, Min(0.05f)] private float _pullBackTime = 0.3f;

        [SerializeField, Min(1)] private int _minTurns = 3;

        [SerializeField, Min(1)] private int _maxTurns = 5;

        [SerializeField, Min(0.5f)] private float _spinDuration = 3.2f;

        [SerializeField, Min(1f)] private float _brakePower = 5f;

        [SerializeField, Min(0f)] private float _maxMissDegrees = 25f;

        [SerializeField, Min(0.1f)] private float _winPauseSeconds = 0.7f;

        [Header("Reel Sounds")]
        [SerializeField] private SfxClip _stepClip;

        [SerializeField] private SfxClip _spinStartClip;

        [SerializeField] private SfxClip _winClip;

        public IReadOnlyList<RouletteSector> Sectors => _sectors;

        public IReadOnlyList<SkinItem> ExclusiveSkins => _rouletteSkins;

        public int SkinSpinMinCost => _skinSpinMinCost;

        public int SkinSpinMaxCost => _skinSpinMaxCost;

        public int SkinSpinCostStep => _skinSpinCostStep;

        public int DuplicateCoinsCompensation => _duplicateCoins;

        public SkinRarityTable RarityTable => _rarityTable;

        public int AdSpinWindowSeconds => _adSpinWindowSeconds;

        public int AdSpinsPerWindow => _adSpinsPerWindow;

        public int FreeSpinCooldownSeconds => _freeSpinCooldownSeconds;

        public float CommonDropChance => _commonDropChance;

        public float RareDropChance => _rareDropChance;

        public float EpicDropChance => _epicDropChance;

        public float LegendaryDropChance => _legendaryDropChance;

        public int ShowcaseRareCount => _showcaseRareCount;

        public int ShowcaseEpicCount => _showcaseEpicCount;

        public int ShowcaseLegendaryCount => _showcaseLegendaryCount;

        public float IdleRotationSpeed => _idleRotationSpeed;

        public float WindBackDegrees => _pullBackDegrees;

        public float WindBackDuration => _pullBackTime;

        public int MinTurns => _minTurns;

        public int MaxTurns => _maxTurns;

        public float SpinDuration => _spinDuration;

        public float SpinEasePower => _brakePower;

        public float SettleMaxDeviationDegrees => _maxMissDegrees;

        public float WinDwellSeconds => _winPauseSeconds;

        public SfxClip StepClip => _stepClip;

        public SfxClip SpinStartClip => _spinStartClip;

        public SfxClip WinClip => _winClip;
    }
}
