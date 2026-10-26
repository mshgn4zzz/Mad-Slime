using Economy;
using Saves;
using Skins;
using System;
using System.Collections.Generic;
namespace Roulette
{
    public class RouletteFactory
    {
        private RouletteConfig _config;
        private IRouletteStorage _storage;
        private ISkinStorage _skinStorage;
        private SkinInventory _inventory;
        private Wallet _wallet;
        private SavesTransaction _transaction;
        private SkinShowcase _showcase;
        private int _dailySlotCount;
        private int _skinSlotCount;

        public RouletteFactory(RouletteConfig config, IRouletteStorage storage, ISkinStorage skinStorage,
            SkinInventory inventory, Wallet wallet, SavesTransaction transaction)
        {
            _config = config;
            _storage = storage;
            _skinStorage = skinStorage;
            _inventory = inventory;
            _wallet = wallet;
            _transaction = transaction;
        }

        public RouletteModel CreateDaily(int slotCount)
        {
            _dailySlotCount = slotCount;
            RouletteAccess access = new RouletteAccess(GetZero, GetFreeRemainSeconds, GetAdSpinsLeft,
                CanSpinDaily, PayDaily, true);
            return new RouletteModel(new RouletteModelDependencies
            {
                Config = _config,
                CreateSlots = CreateDailySlots,
                Access = access,
                ResolvePrize = ResolveDailyPrize,
                Roller = new LootRoller(),
                Inventory = _inventory,
                Wallet = _wallet,
                Transaction = _transaction,
                RefreshShowcase = null
            });
        }

        public RouletteModel CreateSkins(IReadOnlyList<SkinItem> catalog, int slotCount)
        {
            _skinSlotCount = slotCount;
            _showcase = new SkinShowcase(_config, _skinStorage, _inventory, catalog);
            RouletteAccess access = new RouletteAccess(GetSkinPrice, GetZero, GetZero,
                CanSpinSkins, PaySkins, false);
            return new RouletteModel(new RouletteModelDependencies
            {
                Config = _config,
                CreateSlots = CreateSkinSlots,
                Access = access,
                ResolvePrize = ResolveSkinPrize,
                Roller = new LootRoller(),
                Inventory = _inventory,
                Wallet = _wallet,
                Transaction = _transaction,
                RefreshShowcase = _showcase.Refresh
            });
        }

        private List<RouletteSlot> CreateDailySlots()
        {
            List<RouletteSlot> slots = new List<RouletteSlot>();

            if (_config.Sectors.Count != _dailySlotCount)
            {
                                throw new InvalidOperationException("Daily roulette must configure exactly one "
                                        + "reward per wheel sector.");
            }

            for (int i = 0; i < _dailySlotCount; i++)
            {
                RouletteSector sector = _config.Sectors[i];
                bool isHidden = sector.RewardType == RewardType.Skin;
                slots.Add(new RouletteSlot(null, sector.Coins, sector.DropChance, isHidden));
            }

            return slots;
        }

        private List<RouletteSlot> CreateSkinSlots()
        {
            return _showcase.CreateSlots(_skinSlotCount);
        }

        private RoulettePrize ResolveDailyPrize(RouletteSlot slot)
        {
            if (slot.IsHidden == false)
            {
                return new RoulettePrize(null, slot.Coins);
            }

            int skinIndex = UnityEngine.Random.Range(0, _config.ExclusiveSkins.Count);
            return new RoulettePrize(_config.ExclusiveSkins[skinIndex], 0);
        }

        private RoulettePrize ResolveSkinPrize(RouletteSlot slot)
        {
            return new RoulettePrize(slot.Skin, 0);
        }

        private int GetZero()
        {
            return 0;
        }

        private int GetFreeRemainSeconds()
        {
            long remain = _config.FreeSpinCooldownSeconds - (GetNow() - _storage.LastFreeSpinUnixTime);

            if (remain <= 0)
            {
                return 0;
            }

            return (int)Math.Min(remain, int.MaxValue);
        }

        private int GetAdSpinsLeft()
        {
            int used = _storage.CountAdSpinsSince(GetNow() - _config.AdSpinWindowSeconds);
            return Math.Max(0, _config.AdSpinsPerWindow - used);
        }

        private bool CanSpinDaily(bool advertisementGranted)
        {
            if (advertisementGranted == true)
            {
                return GetAdSpinsLeft() > 0;
            }

            return GetFreeRemainSeconds() == 0;
        }

        private void PayDaily(bool advertisementGranted)
        {
            long now = GetNow();

            if (advertisementGranted == true)
            {
                _storage.PruneAdSpinsBefore(now - _config.AdSpinWindowSeconds);
                _storage.RegisterAdSpin(now);
                return;
            }

            _storage.SetLastFreeSpinUnixTime(now);
        }

        private int GetSkinPrice()
        {
            long price = (long)_config.SkinSpinMinCost + (long)_config.SkinSpinCostStep * _storage.SkinSpinCount;

            if (_config.SkinSpinMaxCost > 0)
            {
                price = Math.Min(price, _config.SkinSpinMaxCost);
            }

            return (int)Math.Min(price, int.MaxValue);
        }

        private bool CanSpinSkins(bool advertisementGranted)
        {
            return advertisementGranted == false && _showcase.HasAvailableSkins() == true
                && _wallet.Balance >= GetSkinPrice() && _storage.SkinSpinCount < int.MaxValue;
        }

        private void PaySkins(bool advertisementGranted)
        {
            _wallet.Spend(GetSkinPrice());
            _storage.SetSkinSpinCount(_storage.SkinSpinCount + 1);
        }

        private long GetNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
