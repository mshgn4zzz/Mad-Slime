using System;
using MadSlime.Economy.Logic;
using MadSlime.Saves.Interfaces;
using MadSlime.Upgrades.Data;
using MadSlime.Upgrades.Logic;
using MadSlime.Validation;

namespace MadSlime.UI.Upgrades
{
    public class UpgradePurchaseModel
    {
        private PlayerUpgrades _upgrades;
        private Wallet _wallet;
        private SavesTransaction _transaction;

        public UpgradePurchaseModel(PlayerUpgrades upgrades, Wallet wallet, SavesTransaction transaction)
        {
            ArgumentGuard.CheckNotNull(upgrades, nameof(upgrades));
            ArgumentGuard.CheckNotNull(wallet, nameof(wallet));
            ArgumentGuard.CheckNotNull(transaction, nameof(transaction));

            _upgrades = upgrades;
            _wallet = wallet;
            _transaction = transaction;
        }

        public bool IsPurchaseAvailable(UpgradeType type)
        {
            return _upgrades.IsMaxed(type) == false && _wallet.Balance >= _upgrades.GetNextCost(type);
        }

        public bool IsPurchaseAvailable(PerkType type)
        {
            return _upgrades.IsPerkPurchased(type) == false && _wallet.Balance >= _upgrades.GetPerkCost(type);
        }

        public bool TryPurchase(UpgradeType type)
        {
            if (IsPurchaseAvailable(type) == false)
            {
                return false;
            }

            Purchase(_upgrades.GetNextCost(type), () => _upgrades.PurchaseStepped(type));
            return true;
        }

        public bool TryPurchase(PerkType type)
        {
            if (IsPurchaseAvailable(type) == false)
            {
                return false;
            }

            Purchase(_upgrades.GetPerkCost(type), () => _upgrades.PurchasePerk(type));
            return true;
        }

        private void Purchase(int cost, Action purchaseStep)
        {
            _transaction.Invoke(() =>
            {
                if (cost > 0)
                {
                    _wallet.Spend(cost);
                }

                purchaseStep();
            });
        }
    }
}
