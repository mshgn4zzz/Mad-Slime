using System;
using UnityEngine;
namespace Localization
{
    [Serializable]
    public class LocaleEntry
    {
        [Tooltip("Ключ, по которому код запрашивает строку через Localization.Get.")]
        [SerializeField, InspectorName("Ключ")] private string _key;

        [Tooltip("Русская строка.")]
        [SerializeField, InspectorName("Русский (ru)")] private string _ru;

        [Tooltip("Английская строка. Пусто — фолбэк на русскую.")]
        [SerializeField, InspectorName("English (en)")] private string _en;

        [Tooltip("Турецкая строка. Пусто — фолбэк на английскую.")]
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
