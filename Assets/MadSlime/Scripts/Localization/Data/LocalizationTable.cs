using System.Collections.Generic;
using UnityEngine;

namespace MadSlime.Localization.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Localization Table", fileName = "Localization")]
    public class LocalizationTable : ScriptableObject
    {
        public const string Russian = "ru";
        public const string English = "en";
        public const string Turkish = "tr";

        [SerializeField] private List<LocaleEntry> _entries = new List<LocaleEntry>();

        public IEnumerable<LocaleEntry> Entries => _entries;
    }
}
