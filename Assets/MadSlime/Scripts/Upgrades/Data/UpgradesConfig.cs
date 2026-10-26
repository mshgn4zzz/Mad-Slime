using System;
using System.Collections.Generic;
using UnityEngine;
namespace Upgrades
{
    [CreateAssetMenu(menuName = "Mad Slime/Upgrades Config", fileName = "NewUpgradesConfig")]
    public class UpgradesConfig : ScriptableObject
    {
        [Tooltip("Ступенчатые прокачки: цена ступени = BaseCost * CostMultiplier в степени текущего уровня.")]
        [SerializeField] private List<UpgradeEntry> _upgrades = new List<UpgradeEntry>();

        [Tooltip("Одноразовые покупки: стоят дорого, покупаются один раз, бафф навсегда.")]
        [SerializeField] private List<PerkEntry> _perks = new List<PerkEntry>();

        [Header("Smell")]
        [Tooltip("Цвет аутлайна квотовых предметов (Улучшенный нюх). Уходит в Outline.OutlineColor (QuickOutline).")]
        [SerializeField] private Color _highlightColor = new Color(1f, 0.85f, 0.2f, 1f);

        [Tooltip("Ширина аутлайна квотовых предметов — Outline.OutlineWidth (QuickOutline): "
            + "экструзия силуэта по сглаженным нормалям. 1 ≈ тонкая линия, 10 ≈ жирная обводка.")]
        [SerializeField, Min(0f)] private float _outlineWidth = 4f;

        public IReadOnlyList<UpgradeEntry> Upgrades => _upgrades;
        public IReadOnlyList<PerkEntry> Perks => _perks;
        public Color HighlightColor => _highlightColor;
        public float OutlineWidth => _outlineWidth;

    }

}
