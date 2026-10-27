using System;
using YG;

namespace Saves
{
    public class BalanceStorage : IBalanceStorage
    {
        private SavesCore _core;

        public BalanceStorage(SavesCore core)
        {
            if (core == null)
            {
                throw new ArgumentNullException(nameof(core),
                    "BalanceStorage requires a SavesCore instance.");
            }

            _core = core;
        }

        public int Balance => _core.LoadData().Balance;

        public void SetBalance(int balance)
        {
            if (balance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(balance),
                    balance,
                    "BalanceStorage.SetBalance requires a non-negative balance.");
            }

            SavesYG data = _core.LoadData();

            data.Balance = balance;
            _core.SaveChanges();
        }
    }
}
