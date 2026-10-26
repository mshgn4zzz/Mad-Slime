using Economy;
using Saves;
using Skins;
using System;
using System.Collections.Generic;
namespace Roulette
{
    public class RouletteModel
    {
        private RouletteConfig _config;
        private Func<List<RouletteSlot>> _createSlots;
        private RouletteAccess _access;
        private Func<RouletteSlot, RoulettePrize> _resolvePrize;
        private LootRoller _roller;
        private SkinInventory _inventory;
        private Wallet _wallet;
        private SavesTransaction _transaction;
        private Action _refreshShowcase;
        private List<RouletteSlot> _slots;
        private bool _advertisementGranted;
        private RouletteOutcome _outcome;

        public RouletteConfig Config => _config;
        public IReadOnlyList<RouletteSlot> Slots => _slots;
        public bool HasAdvertising => _access.HasAdvertising;

        public RouletteModel(RouletteModelDependencies dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException(nameof(dependencies));
            }

            if (dependencies.Config == null)
            {
                throw new ArgumentNullException(nameof(dependencies.Config));
            }

            if (dependencies.CreateSlots == null)
            {
                throw new ArgumentNullException(nameof(dependencies.CreateSlots));
            }

            if (dependencies.Access == null)
            {
                throw new ArgumentNullException(nameof(dependencies.Access));
            }

            if (dependencies.ResolvePrize == null)
            {
                throw new ArgumentNullException(nameof(dependencies.ResolvePrize));
            }

            if (dependencies.Roller == null)
            {
                throw new ArgumentNullException(nameof(dependencies.Roller));
            }

            if (dependencies.Inventory == null)
            {
                throw new ArgumentNullException(nameof(dependencies.Inventory));
            }

            if (dependencies.Wallet == null)
            {
                throw new ArgumentNullException(nameof(dependencies.Wallet));
            }

            if (dependencies.Transaction == null)
            {
                throw new ArgumentNullException(nameof(dependencies.Transaction));
            }

            _config = dependencies.Config;
            _createSlots = dependencies.CreateSlots;
            _access = dependencies.Access;
            _resolvePrize = dependencies.ResolvePrize;
            _roller = dependencies.Roller;
            _inventory = dependencies.Inventory;
            _wallet = dependencies.Wallet;
            _transaction = dependencies.Transaction;
            _refreshShowcase = dependencies.RefreshShowcase;
        }

        public void RefreshSlots()
        {
            _slots = _createSlots();
        }

        public int GetPrice()
        {
            return _access.GetPrice();
        }

        public bool HasAvailableSkin()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                RouletteSlot slot = _slots[i];

                if (slot.Skin != null && slot.Weight > 0f && _inventory.IsOpen(slot.Skin.Id) == false)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetFreeRemainSeconds()
        {
            return _access.GetFreeRemainSeconds();
        }

        public int GetAdSpinsLeft()
        {
            return _access.GetAdSpinsLeft();
        }

        public bool CanSpin(bool advertisementGranted)
        {
            if (_slots == null || _slots.Count == 0)
            {
                return false;
            }

            return _access.CanSpin(advertisementGranted);
        }

        public bool Spin(bool advertisementGranted, out RouletteOutcome outcome)
        {
            outcome = null;

            if (CanSpin(advertisementGranted) == false)
            {
                return false;
            }

            _advertisementGranted = advertisementGranted;
            _transaction(ExecuteSpin);
            outcome = _outcome;
            _outcome = null;
            return true;
        }

        private void ExecuteSpin()
        {
            _access.Pay(_advertisementGranted);
            int slotIndex = _roller.Roll(_slots);
            RoulettePrize prize = _resolvePrize(_slots[slotIndex]);

            if (prize.Skin != null)
            {
                if (_inventory.Grant(prize.Skin.Id) == false)
                {
                    prize = new RoulettePrize(null, _config.DuplicateCoinsCompensation);
                }
            }

            if (prize.Coins > 0)
            {
                _wallet.Add(prize.Coins);
            }

            _refreshShowcase?.Invoke();
            _outcome = new RouletteOutcome(slotIndex, prize);
        }
    }
}
