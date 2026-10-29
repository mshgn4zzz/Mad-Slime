using System;
using System.Collections.Generic;
using MadSlime.Skins.Data;
using MadSlime.Upgrades.Data;

namespace MadSlime.Saves.Data
{
    [Serializable]
    public class PlayerSaveData
    {
        public int MusicVolume = 50;
        public int SfxVolume = 35;
        public List<string> OpenSkinIds = new List<string> { SkinInventory.InitialSkinId };
        public List<string> ShowcaseSkinIds = new List<string>();
        public int CurrentLevel = 1;
        public int MaxLevel = 1;
        public int CollectedItemsCount;
        public string Language = "";

        public int Balance;

        public string SelectedSkinId = SkinInventory.InitialSkinId;

        public int SpeedLevel;
        public int AppetiteLevel;
        public int TasteLevel;
        public int MetabolismLevel;

        public List<PerkType> PurchasedPerks = new List<PerkType>();

        public long LastFreeSpinUnixTime;
        public List<long> RouletteAdSpinTimes = new List<long>();
        public int SkinSpinCount;
    }
}
