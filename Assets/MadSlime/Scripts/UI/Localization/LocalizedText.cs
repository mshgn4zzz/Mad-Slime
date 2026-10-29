using System;
using TMPro;
using UnityEngine;

namespace MadSlime.UI.Localization
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [VContainer.Inject] private Localization _localization;

        [SerializeField] private string _key;

        private TMP_Text _text;

        private void Awake()
        {
            if (TryGetComponent(out _text) == false)
            {
                throw new InvalidOperationException($"{name}: TMP_Text is required.");
            }

            if (string.IsNullOrEmpty(_key))
            {
                throw new InvalidOperationException(
                    $"{name}: LocalizedText key is empty. Fill the _key field.");
            }
        }

        private void OnEnable()
        {
            _localization.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void OnDisable()
        {
            _localization.LanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            _text.text = _localization.Get(_key);
        }
    }
}
