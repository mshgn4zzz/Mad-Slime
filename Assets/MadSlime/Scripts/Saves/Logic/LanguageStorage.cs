using System;
using YG;

namespace Saves
{
    public class LanguageStorage : ILanguageStorage
    {
        private SavesCore _core;

        public LanguageStorage(SavesCore core)
        {
            if (core == null)
            {
                throw new ArgumentNullException(nameof(core),
                    "LanguageStorage requires a SavesCore instance.");
            }

            _core = core;
        }

        public string Language => _core.LoadData().Language;

        public void SetLanguage(string language)
        {
            if (language == null)
            {
                throw new ArgumentNullException(nameof(language),
                    "LanguageStorage.SetLanguage requires a language code.");
            }

            _core.LoadData().Language = language;
            _core.SaveChanges();
        }
    }
}
