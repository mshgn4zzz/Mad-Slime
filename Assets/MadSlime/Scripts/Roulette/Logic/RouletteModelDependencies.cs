using Economy;
using Saves;
using Skins;
using System;
using System.Collections.Generic;
namespace Roulette
{
    public class RouletteModelDependencies
    {
        public RouletteConfig Config { get; set; }

        public Func<List<RouletteSlot>> CreateSlots { get; set; }

        public RouletteAccess Access { get; set; }

        public Func<RouletteSlot, RoulettePrize> ResolvePrize { get; set; }

        public LootRoller Roller { get; set; }

        public SkinInventory Inventory { get; set; }

        public Wallet Wallet { get; set; }

        public SavesTransaction Transaction { get; set; }

        public Action RefreshShowcase { get; set; }
    }
}
