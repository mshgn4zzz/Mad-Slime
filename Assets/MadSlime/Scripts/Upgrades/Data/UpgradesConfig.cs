using System;
using System.Collections.Generic;
using UnityEngine;
namespace Upgrades
{
    [CreateAssetMenu(menuName = "Mad Slime/Upgrades Config", fileName = "NewUpgradesConfig")]
    public class UpgradesConfig : ScriptableObject
    {
        [SerializeField] private List<UpgradeEntry> _upgrades = new List<UpgradeEntry>();

        [SerializeField] private List<PerkEntry> _perks = new List<PerkEntry>();

        [Header("Smell")]
        [SerializeField] private Color _highlightColor = new Color(1f, 0.85f, 0.2f, 1f);

        [SerializeField, Min(0f)] private float _outlineWidth = 4f;

        public IReadOnlyList<UpgradeEntry> Upgrades => _upgrades;
        public IReadOnlyList<PerkEntry> Perks => _perks;
        public Color HighlightColor => _highlightColor;
        public float OutlineWidth => _outlineWidth;

    }

}
