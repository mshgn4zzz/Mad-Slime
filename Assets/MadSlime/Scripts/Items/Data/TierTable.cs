using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Mad Slime/Tier Table", fileName = "NewTierTable")]
    public class TierTable : ScriptableObject
    {
        [SerializeField] private List<TierEntry> _entries = new List<TierEntry>();

        public IReadOnlyList<TierEntry> Entries => _entries;
    }
}
