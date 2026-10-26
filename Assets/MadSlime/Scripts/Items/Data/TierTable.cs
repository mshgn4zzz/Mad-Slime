using System;
using System.Collections.Generic;
using UnityEngine;
namespace Items
{
    [CreateAssetMenu(menuName = "Mad Slime/Tier Table", fileName = "NewTierTable")]
    public class TierTable : ScriptableObject
    {
        [Tooltip("Строки тиров: тир предмета, его масштаб в мире и масса, которую он даёт игроку.")]
        [SerializeField] private List<TierEntry> _entries = new List<TierEntry>();

        public IReadOnlyList<TierEntry> Entries => _entries;

    }

}
