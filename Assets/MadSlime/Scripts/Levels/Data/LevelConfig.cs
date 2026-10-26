using Items;
using System.Collections.Generic;
using UnityEngine;
namespace Levels
{
    [CreateAssetMenu(menuName = "Mad Slime/Level Config", fileName = "NewLevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Tooltip("Тема уровня: материал пола и текстура формы для сцены заливки.")]
        [SerializeField] private LevelTheme _theme;

        [Tooltip("Набор предметов и их вариантов для генерации уровня.")]
        [SerializeField] private PropSet _propSet;

        [Tooltip("Минимальный тир предметов, спавнящихся на уровне.")]
        [SerializeField] private SizeTier _minTier = SizeTier.Small;

        [Tooltip("Максимальный тир предметов на уровне.")]
        [SerializeField] private SizeTier _maxTier = SizeTier.Huge;

        [Tooltip("Длительность таймера уровня (с).")]
        [SerializeField] private float _timerDuration = 90f;

        [Header("Quota Generation")]
        [Tooltip("Минимум разных типов предметов в квоте.")]
        [SerializeField, Min(1)] private int _quotaTypesMin = 1;

        [Tooltip("Максимум разных типов предметов в квоте.")]
        [SerializeField, Min(1)] private int _quotaTypesMax = 3;

        [Tooltip("Минимальное количество предметов по одному типу квоты.")]
        [SerializeField, Min(1)] private int _quotaTargetMin = 3;

        [Tooltip("Максимальное количество предметов по одному типу квоты.")]
        [SerializeField, Min(1)] private int _quotaTargetMax = 8;

                [Tooltip("Предпочтительный максимум типов одного тира. При нехватке разнообразия минимум типов "
                    + "квоты имеет приоритет.")]
        [SerializeField, Min(1)] private int _quotaMaxSameTier = 2;

        public LevelTheme Theme => _theme;
        public PropSet PropSet => _propSet;
        public SizeTier MinTier => _minTier;
        public SizeTier MaxTier => _maxTier;
        public float TimerDuration => _timerDuration;
        public int QuotaTypesMin => _quotaTypesMin;
        public int QuotaTypesMax => _quotaTypesMax;
        public int QuotaTargetMin => _quotaTargetMin;
        public int QuotaTargetMax => _quotaTargetMax;
        public int QuotaMaxSameTier => _quotaMaxSameTier;
    }
}
