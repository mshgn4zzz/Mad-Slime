using MadSlime.Saves.Interfaces;
using MadSlime.Validation;
using YG;

namespace MadSlime.Saves.Logic
{
    public class BalanceStorage : IBalanceStorage
    {
        private SavesCore _core;

        public BalanceStorage(SavesCore core)
        {
            ArgumentGuard.CheckNotNull(core, nameof(core));

            _core = core;
        }

        public int Balance => _core.LoadData().Balance;

        public void SetBalance(int balance)
        {
            ArgumentGuard.CheckNonNegative(balance, nameof(balance));

            SavesYG data = _core.LoadData();

            data.Balance = balance;
            _core.SaveChanges();
        }
    }
}
