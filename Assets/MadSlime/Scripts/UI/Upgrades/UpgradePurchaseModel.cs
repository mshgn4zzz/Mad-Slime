using Economy;
using Saves;
using System;
using Upgrades;
using Game;
namespace UI.Upgrades
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

        public bool CanPurchase(UpgradeType type)
        {
            return _upgrades.IsMaxed(type) == false && _wallet.Balance >= _upgrades.GetNextCost(type);
        }

        public bool CanPurchase(PerkType type)
        {
            return _upgrades.IsPerkPurchased(type) == false && _wallet.Balance >= _upgrades.GetPerkCost(type);
        }

        public bool Purchase(UpgradeType type)
        {
            if (CanPurchase(type) == false)
            {
                return false;
            }

            return PurchaseChecked(_upgrades.GetNextCost(type), () => _upgrades.PurchaseStepped(type));
        }

        public bool Purchase(PerkType type)
        {
            if (CanPurchase(type) == false)
            {
                return false;
            }

            return PurchaseChecked(_upgrades.GetPerkCost(type), () => _upgrades.PurchasePerk(type));
        }

        private bool PurchaseChecked(int cost, Action purchaseStep)
        {
            _transaction.Invoke(() =>
            {
                if (cost > 0)
                {
                    _wallet.Spend(cost);
                }

                purchaseStep();
            });

            return true;
        }
    }
}
