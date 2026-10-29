namespace MadSlime.Saves.Interfaces
{
    public interface IBalanceStorage
    {
        int Balance { get; }

        void SetBalance(int balance);
    }
}
