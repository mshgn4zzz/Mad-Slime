using Items;
using System.Collections.Generic;
using UnityEngine;
namespace Levels
{
    [CreateAssetMenu(menuName = "Mad Slime/Level Config", fileName = "NewLevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private LevelTheme _theme;

        [SerializeField] private PropSet _propSet;

        [SerializeField] private SizeTier _minTier = SizeTier.Small;

        [SerializeField] private SizeTier _maxTier = SizeTier.Huge;

        [SerializeField] private float _levelTime = 90f;

        [Header("Quota Generation")]
        [SerializeField, Min(1)] private int _quotaTypesMin = 1;

        [SerializeField, Min(1)] private int _quotaTypesMax = 3;

        [SerializeField, Min(1)] private int _quotaTargetMin = 3;

        [SerializeField, Min(1)] private int _quotaTargetMax = 8;

        [SerializeField, Min(1)] private int _quotaMaxSameTier = 2;

        public LevelTheme Theme => _theme;
        public PropSet PropSet => _propSet;
        public SizeTier MinTier => _minTier;
        public SizeTier MaxTier => _maxTier;
        public float TimerDuration => _levelTime;
        public int QuotaTypesMin => _quotaTypesMin;
        public int QuotaTypesMax => _quotaTypesMax;
        public int QuotaTargetMin => _quotaTargetMin;
        public int QuotaTargetMax => _quotaTargetMax;
        public int QuotaMaxSameTier => _quotaMaxSameTier;
    }
}
