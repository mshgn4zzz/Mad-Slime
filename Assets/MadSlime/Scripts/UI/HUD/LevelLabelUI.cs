using System;
using TMPro;
using UnityEngine;

namespace UI.HUD
{
    using Localization = global::Localization.Localization;

    public class LevelLabelUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _labelText;

        private int _level;

        private void Awake()
        {
            if (_labelText == null)
            {
                throw new InvalidOperationException(
                    $"{name}: LabelText is not assigned. Drag a TMP_Text into the _labelText field.");
            }
        }

        private void OnEnable()
        {
            Localization.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void OnDisable()
        {
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        public void SetLevel(int level)
        {
            if (level <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level));
            }

            _level = level;
            OnLanguageChanged();
        }

        private void OnLanguageChanged()
        {
            if (_level <= 0)
            {
                return;
            }

            _labelText.text = string.Format(Localization.Get("level_label"), _level);
        }
    }
}
