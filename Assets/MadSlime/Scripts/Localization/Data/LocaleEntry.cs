using System;
using UnityEngine;
namespace Localization
{
    [Serializable]
    public class LocaleEntry
    {
        [SerializeField, InspectorName("Ключ")] private string _key;

        [SerializeField, InspectorName("Русский (ru)")] private string _ru;

        [SerializeField, InspectorName("English (en)")] private string _en;

        [SerializeField, InspectorName("Türkçe (tr)")] private string _tr;

        public string Key => _key;

        public string Get(string language)
        {
            if (language == LocalizationTable.English)
            {
                return Pick(_en, _ru);
            }

            if (language == LocalizationTable.Turkish)
            {
                return Pick(_tr, _en);
            }

            return Pick(_ru, _en);
        }

        private static string Pick(string preferred, string fallback)
        {
            if (string.IsNullOrEmpty(preferred) == false)
            {
                return preferred;
            }

            if (string.IsNullOrEmpty(fallback) == false)
            {
                return fallback;
            }

            return string.Empty;
        }
    }
}
