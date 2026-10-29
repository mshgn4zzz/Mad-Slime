using MadSlime.Saves.Interfaces;
using MadSlime.Validation;

namespace MadSlime.Saves.Logic
{
    public class LanguageStorage : ILanguageStorage
    {
        private SavesCore _core;

        public LanguageStorage(SavesCore core)
        {
            ArgumentGuard.CheckNotNull(core, nameof(core));

            _core = core;
        }

        public string Language => _core.LoadData().Language;

        public void SetLanguage(string language)
        {
            ArgumentGuard.CheckNotNull(language, nameof(language));

            _core.LoadData().Language = language;
            _core.SaveChanges();
        }
    }
}
