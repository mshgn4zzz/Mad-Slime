using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Localization
{
    using Localization = global::Localization.Localization;

    public class LanguageSwitcher : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _flag;
        [SerializeField] private Sprite _russianFlag;
        [SerializeField] private Sprite _englishFlag;
        [SerializeField] private Sprite _turkishFlag;

        private void Awake()
        {
            if (_button == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Button is not assigned. Drag a Button into the _button field.");
            }

            if (_label == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Label is not assigned. Drag a TMP_Text into the _label field.");
            }

            if (_flag == null || _russianFlag == null || _englishFlag == null || _turkishFlag == null)
            {
                throw new InvalidOperationException($"{name}: Language flag image and sprites must be assigned.");
            }
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
            Localization.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        private void OnButtonClick()
        {
            Localization.CycleLanguage();
        }

        private void OnLanguageChanged()
        {
            _label.text = Localization.Get($"lang_self_{Localization.CurrentLanguage}");

            switch (Localization.CurrentLanguage)
            {
                case Localization.Russian:
                    _flag.sprite = _russianFlag;
                    break;

                case Localization.English:
                    _flag.sprite = _englishFlag;
                    break;

                case Localization.Turkish:
                    _flag.sprite = _turkishFlag;
                    break;

                default:
                                        throw new InvalidOperationException($"{name}: Unsupported language "
                                                + $"{Localization.CurrentLanguage}.");
            }
        }
    }
}
