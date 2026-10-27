using System;
using System.Collections.Generic;
using Upgrades;
using YG;

namespace Saves
{
    public class UpgradesStorage : IUpgradesStorage
    {
        private SavesCore _core;

        public UpgradesStorage(SavesCore core)
        {
            if (core == null)
            {
                throw new ArgumentNullException(nameof(core),
                    "UpgradesStorage requires a SavesCore instance.");
            }

            _core = core;
        }

        public int GetUpgradeLevel(UpgradeType type)
        {
            SavesYG data = _core.LoadData();

            switch (type)
            {
                case UpgradeType.Speed:
                    return data.SpeedLevel;

                case UpgradeType.Appetite:
                    return data.AppetiteLevel;

                case UpgradeType.Taste:
                    return data.TasteLevel;

                case UpgradeType.Metabolism:
                    return data.MetabolismLevel;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        "UpgradesStorage.GetUpgradeLevel received an unknown upgrade type.");
            }
        }

        public void SetUpgradeLevel(UpgradeType type, int level)
        {
            if (level < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(level),
                    level,
                    "UpgradesStorage.SetUpgradeLevel requires a non-negative level.");
            }

            SavesYG data = _core.LoadData();

            switch (type)
            {
                case UpgradeType.Speed:
                    data.SpeedLevel = level;
                    break;

                case UpgradeType.Appetite:
                    data.AppetiteLevel = level;
                    break;

                case UpgradeType.Taste:
                    data.TasteLevel = level;
                    break;

                case UpgradeType.Metabolism:
                    data.MetabolismLevel = level;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        "UpgradesStorage.SetUpgradeLevel received an unknown upgrade type.");
            }

            _core.SaveChanges();
        }

        public bool IsPerkPurchased(PerkType type)
        {
            return _core.LoadData().PurchasedPerks.Contains(type);
        }

        public void PurchasePerk(PerkType type)
        {
            List<PerkType> purchasedPerks = _core.LoadData().PurchasedPerks;

            if (purchasedPerks.Contains(type) == true)
            {
                throw new InvalidOperationException(
                    $"UpgradesStorage.PurchasePerk: perk '{type}' is already purchased.");
            }

            purchasedPerks.Add(type);
            _core.SaveChanges();
        }
    }
}
