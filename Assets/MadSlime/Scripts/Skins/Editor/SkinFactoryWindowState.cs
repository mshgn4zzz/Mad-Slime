using System;
using MadSlime.Skins.Data;

namespace MadSlime.Skins.Editor
{
    [Serializable]
    public class SkinFactoryWindowState
    {
        public string ShopContentPath;
        public string SkinsPath;
        public string PrefabsPath;
        public string IconsPath;
        public SkinRarity Rarity;
        [UnityEngine.Serialization.FormerlySerializedAs("ForceIcon")]
        public bool IsIconRegenerationForced;
    }
}
