using System;
using MadSlime.Saves.Interfaces;
using MadSlime.Validation;

namespace MadSlime.Economy.Logic
{
    public class Wallet
    {
        private readonly IBalanceStorage _balanceStorage;

        public Wallet(IBalanceStorage balanceStorage)
        {
            _balanceStorage = balanceStorage;

            ArgumentGuard.CheckNotNull(_balanceStorage, nameof(balanceStorage));
        }

        public event Action<int, int> BalanceChanged;

        public int Balance => _balanceStorage.Balance;

        public void Add(int amount)
        {
            ArgumentGuard.CheckPositive(amount, nameof(amount));

            SetBalance(_balanceStorage.Balance + amount);
        }

        public void Spend(int amount)
        {
            ArgumentGuard.CheckPositive(amount, nameof(amount));

            if (_balanceStorage.Balance < amount)
            {
                throw new InvalidOperationException(
                    $"Wallet.Spend failed: balance {_balanceStorage.Balance} is less than required {amount}.");
            }

            SetBalance(_balanceStorage.Balance - amount);
        }

        private void SetBalance(int balance)
        {
            int previousBalance = _balanceStorage.Balance;

            _balanceStorage.SetBalance(balance);

            BalanceChanged?.Invoke(previousBalance, balance);
        }
    }
}
