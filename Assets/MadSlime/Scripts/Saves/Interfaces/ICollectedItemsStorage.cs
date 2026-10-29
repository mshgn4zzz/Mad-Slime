namespace MadSlime.Saves.Interfaces
{
    public interface ICollectedItemsStorage
    {
        int CollectedItemsCount { get; }

        void RegisterCollectedItem();

        void CommitCollectedItems();
    }
}
