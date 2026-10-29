using System;

namespace MadSlime.Localization.Interfaces
{
    public interface ILanguageProvider
    {
        event Action<string> LanguageSwitched;

        string Language { get; }
    }
}
