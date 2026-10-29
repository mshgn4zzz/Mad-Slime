using System;
using MadSlime.Localization.Data;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Localization.Logic
{
    public class Localization
    {
        public const string Russian = LocalizationTable.Russian;
        public const string English = LocalizationTable.English;
        public const string Turkish = LocalizationTable.Turkish;

        private string[] _languageOrder = { Russian, English, Turkish };

        private LocalizationTable _table;
        private string _language = Russian;

        public event Action LanguageChanged;

        public string CurrentLanguage => _language;

        public void Initialize(LocalizationTable table, string savedLanguage)
        {
            ArgumentGuard.CheckNotNull(table, nameof(table));

            _table = table;
            _language = ResolveLanguage(savedLanguage);
        }

        public string Get(string key)
        {
            if (_table == null)
            {
                throw new InvalidOperationException(
                    "Localization is not initialized. LocalizationService must exist on the ProjectScope.");
            }

            return GetTranslation(key);
        }

        public void SetLanguage(string language)
        {
            string resolved = ResolveLanguage(language);

            if (resolved == _language)
            {
                return;
            }

            _language = resolved;
            LanguageChanged?.Invoke();
        }

        public void CycleLanguage()
        {
            int next = (Array.IndexOf(_languageOrder, _language) + 1) % _languageOrder.Length;

            SetLanguage(_languageOrder[next]);
        }

        private string GetTranslation(string key)
        {
            foreach (LocaleEntry entry in _table.Entries)
            {
                if (entry.Key != key)
                {
                    continue;
                }

                return entry.Get(_language);
            }

            return key;
        }

        private string ResolveLanguage(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return FromSystemLanguage();
            }

            for (int i = 0; i < _languageOrder.Length; i++)
            {
                if (_languageOrder[i] == language)
                {
                    return language;
                }
            }

            return English;
        }

        private string FromSystemLanguage()
        {
            switch (Application.systemLanguage)
            {
                case UnityEngine.SystemLanguage.Russian:
                    return Russian;

                case UnityEngine.SystemLanguage.Turkish:
                    return Turkish;

                default:
                    return English;
            }
        }
    }
}
