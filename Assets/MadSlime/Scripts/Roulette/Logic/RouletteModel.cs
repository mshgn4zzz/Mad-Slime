using System;
using System.Collections.Generic;
using MadSlime.Economy.Logic;
using MadSlime.Roulette.Data;
using MadSlime.Saves.Interfaces;
using MadSlime.Skins.Data;
using MadSlime.Validation;

namespace MadSlime.Roulette.Logic
{
    public class RouletteModel
    {
        private RouletteConfig _config;
        private Func<List<RouletteSlot>> _createSlots;
        private RouletteAccess _access;
        private Func<RouletteSlot, RoulettePrize> _resolvePrize;
        private RouletteSlotCalculator _slotCalculator;
        private SkinInventory _inventory;
        private Wallet _wallet;
        private SavesTransaction _transaction;
        private Action _refreshShowcase;
        private List<RouletteSlot> _slots;
        private bool _isAdvertisementGranted;
        private RouletteOutcome _outcome;

        public RouletteModel(RouletteModelDependencies dependencies)
        {
            ArgumentGuard.CheckNotNull(dependencies, nameof(dependencies));
            ArgumentGuard.CheckNotNull(dependencies.Config, nameof(dependencies.Config));
            ArgumentGuard.CheckNotNull(dependencies.CreateSlots, nameof(dependencies.CreateSlots));
            ArgumentGuard.CheckNotNull(dependencies.Access, nameof(dependencies.Access));
            ArgumentGuard.CheckNotNull(dependencies.ResolvePrize, nameof(dependencies.ResolvePrize));
            ArgumentGuard.CheckNotNull(dependencies.SlotCalculator, nameof(dependencies.SlotCalculator));
            ArgumentGuard.CheckNotNull(dependencies.Inventory, nameof(dependencies.Inventory));
            ArgumentGuard.CheckNotNull(dependencies.Wallet, nameof(dependencies.Wallet));
            ArgumentGuard.CheckNotNull(dependencies.Transaction, nameof(dependencies.Transaction));

            _config = dependencies.Config;
            _createSlots = dependencies.CreateSlots;
            _access = dependencies.Access;
            _resolvePrize = dependencies.ResolvePrize;
            _slotCalculator = dependencies.SlotCalculator;
            _inventory = dependencies.Inventory;
            _wallet = dependencies.Wallet;
            _transaction = dependencies.Transaction;
            _refreshShowcase = dependencies.RefreshShowcase;
        }

        public RouletteConfig Config => _config;

        public IReadOnlyList<RouletteSlot> Slots => _slots;

        public bool IsAdvertisingAvailable => _access.IsAdvertisingAvailable;

        public void RefreshSlots()
        {
            _slots = _createSlots();
        }

        public int GetPrice()
        {
            return _access.GetPrice();
        }

        public bool IsSkinAvailable()
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

        public bool IsSpinAvailable(bool isAdvertisementGranted)
        {
            if (_slots == null || _slots.Count == 0)
            {
                return false;
            }

            return _access.IsSpinAvailable(isAdvertisementGranted);
        }

        public bool TrySpin(bool isAdvertisementGranted, out RouletteOutcome outcome)
        {
            outcome = null;

            if (IsSpinAvailable(isAdvertisementGranted) == false)
            {
                return false;
            }

            _isAdvertisementGranted = isAdvertisementGranted;
            _transaction(ExecuteSpin);
            outcome = _outcome;
            _outcome = null;
            return true;
        }

        private void ExecuteSpin()
        {
            _access.Pay(_isAdvertisementGranted);

            int slotIndex = _slotCalculator.CalculateIndex(_slots);
            RoulettePrize prize = _resolvePrize(_slots[slotIndex]);

            if (prize.Skin != null)
            {
                if (_inventory.TryGrant(prize.Skin.Id) == false)
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
