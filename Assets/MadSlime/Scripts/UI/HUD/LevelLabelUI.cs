using MadSlime.Validation;
using TMPro;
using UnityEngine;

namespace MadSlime.UI.HUD
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    public class LevelLabelUI : MonoBehaviour
    {
        [VContainer.Inject] private Localization _localization;

        [SerializeField] private TMP_Text _labelText;

        private int _level;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_labelText, name, nameof(_labelText));
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

        public void SetLevel(int level)
        {
            ArgumentGuard.CheckPositive(level, nameof(level));

            _level = level;
            OnLanguageChanged();
        }

        private void OnLanguageChanged()
        {
            if (_level <= 0)
            {
                return;
            }

            _labelText.text = string.Format(_localization.Get("level_label"), _level);
        }
    }
}
