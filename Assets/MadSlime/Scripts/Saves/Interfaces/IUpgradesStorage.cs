using MadSlime.Upgrades.Data;

namespace MadSlime.Saves.Interfaces
{
    public interface IUpgradesStorage
    {
        int GetUpgradeLevel(UpgradeType type);

        void SetUpgradeLevel(UpgradeType type, int level);

        bool IsPerkPurchased(PerkType type);

        void PurchasePerk(PerkType type);
    }
}
