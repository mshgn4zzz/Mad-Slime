using System.Collections.Generic;
using UnityEngine;

namespace MadSlime.Skins.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Skin Rarity Table", fileName = "SkinRarityTable")]
    public class SkinRarityTable : ScriptableObject
    {
        [SerializeField] private List<RaritySettings> _settings = new List<RaritySettings>();

        public IReadOnlyList<RaritySettings> Settings => _settings;
    }
}
