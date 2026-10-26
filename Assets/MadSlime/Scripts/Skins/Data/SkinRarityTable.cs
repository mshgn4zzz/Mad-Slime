using System;
using System.Collections.Generic;
using UnityEngine;
namespace Skins
{
    [CreateAssetMenu(menuName = "Mad Slime/Skin Rarity Table", fileName = "SkinRarityTable")]
    public class SkinRarityTable : ScriptableObject
    {
        [Tooltip("Настройки редкостей: цвета плашек карточек.")]
        [SerializeField] private List<RaritySettings> _settings = new List<RaritySettings>();

        public IReadOnlyList<RaritySettings> Settings => _settings;

    }

}
