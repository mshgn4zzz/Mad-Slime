using System;
using UnityEngine;
namespace Skins
{
    [Serializable]
    public class RaritySettings
    {
        [Tooltip("Редкость, к которой относятся настройки.")]
        [SerializeField] private SkinRarity _rarity = SkinRarity.Common;

        [Tooltip("Цвет плашки карточки скина в альбоме.")]
        [SerializeField] private Color _plateColor = Color.white;

        public SkinRarity Rarity => _rarity;

        public Color PlateColor => _plateColor;
    }
}
