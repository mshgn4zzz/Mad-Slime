using System;

namespace MadSlime.Items.Editor
{
    [Serializable]
    public class ItemFactoryWindowState
    {
        public string ModelsPath;
        public string PrefabsPath;
        public string DefinitionsPath;
        public string IconsPath;
        public string PropSetPath;
        public string TierTablePath;
        public string GhostMaterialPath;
        public string GhostFadeConfigPath;
        [UnityEngine.Serialization.FormerlySerializedAs("ForceIcons")]
        public bool IsIconRegenerationForced;
    }
}
