using System;
using System.Collections.Generic;
using UnityEngine;

namespace Localization
{
    [CreateAssetMenu(menuName = "Mad Slime/Localization Table", fileName = "Localization")]
    public class LocalizationTable : ScriptableObject
    {
        public const string Russian = "ru";
        public const string English = "en";
        public const string Turkish = "tr";

        [SerializeField] private List<LocaleEntry> _entries = new List<LocaleEntry>();

        public string Get(string key, string language)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Key != key)
                {
                    continue;
                }

                return _entries[i].Get(language);
            }

            return key;
        }

        public IEnumerable<LocaleEntry> Entries => _entries;
    }
}
