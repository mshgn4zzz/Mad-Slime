using System;
using UnityEngine;

namespace MadSlime.Skins.Data
{
    [Serializable]
    public class RaritySettings
    {
        [SerializeField] private SkinRarity _rarity = SkinRarity.Common;

        [SerializeField] private Color _plateColor = Color.white;

        public SkinRarity Rarity => _rarity;

        public Color PlateColor => _plateColor;
    }
}
