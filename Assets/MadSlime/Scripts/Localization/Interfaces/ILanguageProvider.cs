using System;
namespace Localization
{
    public interface ILanguageProvider
    {
        string Language { get; }

        event Action<string> LanguageSwitched;
    }
}
