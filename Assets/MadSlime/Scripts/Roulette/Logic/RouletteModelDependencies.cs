using System;
using System.Collections.Generic;
using MadSlime.Economy.Logic;
using MadSlime.Roulette.Data;
using MadSlime.Saves.Interfaces;
using MadSlime.Skins.Data;

namespace MadSlime.Roulette.Logic
{
    public class RouletteModelDependencies
    {
        public RouletteConfig Config { get; set; }

        public Func<List<RouletteSlot>> CreateSlots { get; set; }

        public RouletteAccess Access { get; set; }

        public Func<RouletteSlot, RoulettePrize> ResolvePrize { get; set; }

        public RouletteSlotCalculator SlotCalculator { get; set; }

        public SkinInventory Inventory { get; set; }

        public Wallet Wallet { get; set; }

        public SavesTransaction Transaction { get; set; }

        public Action RefreshShowcase { get; set; }
    }
}
