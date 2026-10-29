using System;
using MadSlime.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Localization
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    public class LanguageSwitcher : MonoBehaviour
    {
        [VContainer.Inject] private Localization _localization;

        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _flag;
        [SerializeField] private Sprite _russianFlag;
        [SerializeField] private Sprite _englishFlag;
        [SerializeField] private Sprite _turkishFlag;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_button, name, nameof(_button));

            ArgumentGuard.CheckAssigned(_label, name, nameof(_label));

            if (_flag == null || _russianFlag == null || _englishFlag == null || _turkishFlag == null)
            {
                throw new InvalidOperationException($"{name}: Language flag image and sprites must be assigned.");
            }
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
            _localization.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
            _localization.LanguageChanged -= OnLanguageChanged;
        }

        private void OnButtonClick()
        {
            _localization.CycleLanguage();
        }

        private void OnLanguageChanged()
        {
            _label.text = _localization.Get($"lang_self_{_localization.CurrentLanguage}");

            switch (_localization.CurrentLanguage)
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
                    + $"{_localization.CurrentLanguage}.");
            }
        }
    }
}
