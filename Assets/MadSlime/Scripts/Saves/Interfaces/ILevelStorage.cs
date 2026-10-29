namespace MadSlime.Saves.Interfaces
{
    public interface ILevelStorage
    {
        int CurrentLevel { get; }

        int MaxLevel { get; }

        void SetLevelProgress(int currentLevel, int maxLevel);
    }
}
