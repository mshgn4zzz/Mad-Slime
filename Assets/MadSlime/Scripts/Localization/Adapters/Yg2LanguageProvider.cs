using System;
using MadSlime.Localization.Interfaces;
using YG;

namespace MadSlime.Localization.Adapters
{
    public class Yg2LanguageProvider : ILanguageProvider
    {
        public event Action<string> LanguageSwitched
        {
            add
            {
                YG2.onSwitchLang += value;
            }
            remove
            {
                YG2.onSwitchLang -= value;
            }
        }

        public string Language => YG2.lang;
    }
}
