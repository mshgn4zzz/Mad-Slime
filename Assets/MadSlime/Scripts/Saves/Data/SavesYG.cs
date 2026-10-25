using System.Collections.Generic;
using Upgrades;

namespace YG
{
    public partial class SavesYG
    {
        public int CurrentLevel = 1;
        public int MaxLevel = 1;
        public int CollectedItemsCount;
        public string Language = "";

        // Volumes are 0..100 percents; legacy saves stored 0..1 fractions (migrated on read).
        public float musicVolume = 50f;
        public float sfxVolume = 35f;

        public int Balance;

        public string SelectedSkinId = "Slime";
        public List<string> _openSkinIds = new List<string> { "Slime" };
        public List<string> _showcaseSkinIds = new List<string>();

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
