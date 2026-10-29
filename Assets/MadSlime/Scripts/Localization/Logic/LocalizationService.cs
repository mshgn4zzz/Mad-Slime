using MadSlime.Localization.Data;
using MadSlime.Localization.Interfaces;
using MadSlime.Saves.Interfaces;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Localization.Logic
{
    public class LocalizationService : MonoBehaviour
    {
        [SerializeField] private LocalizationTable _table;

        private Localization _localization = new Localization();

        private ILanguageStorage _languageStorage;
        private ISavesReadiness _savesReadiness;
        private ILanguageProvider _languageProvider;
        private bool _isPersistenceSuppressed;

        public Localization Translations => _localization;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_table, name, nameof(_table));
        }

        private void OnEnable()
        {
            ArgumentGuard.CheckAssigned(_languageStorage, name, nameof(_languageStorage));
            ArgumentGuard.CheckAssigned(_savesReadiness, name, nameof(_savesReadiness));
            ArgumentGuard.CheckAssigned(_languageProvider, name, nameof(_languageProvider));

            _localization.Initialize(_table, "");
            _localization.LanguageChanged += OnLanguageChanged;
            _savesReadiness.Prepared += OnSavesReady;
            _languageProvider.LanguageSwitched += OnYandexLangChanged;
            ApplyLanguage();
        }

        private void OnDisable()
        {
            _localization.LanguageChanged -= OnLanguageChanged;

            if (_savesReadiness != null)
            {
                _savesReadiness.Prepared -= OnSavesReady;
            }

            if (_languageProvider != null)
            {
                _languageProvider.LanguageSwitched -= OnYandexLangChanged;
            }
        }

        [Inject]
        public void Construct(
            ILanguageStorage languageStorage,
            ISavesReadiness savesReadiness,
            ILanguageProvider languageProvider)
        {
            _languageStorage = languageStorage;
            _savesReadiness = savesReadiness;
            _languageProvider = languageProvider;
        }

        private void ApplyLanguage()
        {
            if (_savesReadiness.IsReady == false)
            {
                SetSuppressedLanguage(_languageProvider.Language);
                return;
            }

            string savedLanguage = _languageStorage.Language;

            if (string.IsNullOrEmpty(savedLanguage) == false)
            {
                _localization.SetLanguage(savedLanguage);
                return;
            }

            SetSuppressedLanguage(_languageProvider.Language);
        }

        private void SetSuppressedLanguage(string language)
        {
            _isPersistenceSuppressed = true;
            _localization.SetLanguage(language);
            _isPersistenceSuppressed = false;
        }

        private void OnSavesReady()
        {
            ApplyLanguage();
        }

        private void OnYandexLangChanged(string language)
        {
            if (_savesReadiness.IsReady && string.IsNullOrEmpty(_languageStorage.Language) == false)
            {
                return;
            }

            SetSuppressedLanguage(language);
        }

        private void OnLanguageChanged()
        {
            if (_isPersistenceSuppressed)
            {
                return;
            }

            _languageStorage.SetLanguage(_localization.CurrentLanguage);
        }
    }
}
