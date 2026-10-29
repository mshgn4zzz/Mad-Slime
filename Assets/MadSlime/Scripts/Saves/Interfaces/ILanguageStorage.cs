namespace MadSlime.Saves.Interfaces
{
    public interface ILanguageStorage
    {
        string Language { get; }

        void SetLanguage(string language);
    }
}
