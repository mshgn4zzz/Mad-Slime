using Items;
using Levels;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Items.Editor
{
    public partial class ItemPropFactory : EditorWindow
    {
        private const string PrefsKey = "MadSlime.ItemPropFactory";
        private const float NormalizedModelSize = 0.15f;
        private const int IconSize = 256;

        private const string TierField = "_tier";
        private const string IconField = "_icon";
        private const string DefinitionField = "_definition";
        private const string ColliderField = "_collider";
        private const string GhostMaterialField = "_ghostMaterial";
        private const string GhostFadeConfigField = "_ghostFadeConfig";
        private const string PropsField = "_props";
        private const string VariantsField = "_variants";
        private const string PrefabField = "_prefab";

        private DefaultAsset _modelsFolder;
        private DefaultAsset _prefabsFolder;
        private DefaultAsset _definitionsFolder;
        private DefaultAsset _iconsFolder;
        private PropSet _propSet;
        private TierTable _tierTable;
        private Material _ghostMaterial;
        private GhostFadeConfig _ghostFadeConfig;
        private bool _forceIcons;
        private int _iconLayer;

        private static void Open()
        {
            GetWindow<ItemPropFactory>("Prop Factory");
        }

        private void OnEnable()
        {
            LoadState();
        }

        private void OnDisable()
        {
            SaveState();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Point the fields at folders and press Generate — any open scene is fine, the icon camera is "
                    + $"layer-isolated and captures only the model. For every model prefab in the Models Folder "
                            + $"the tool renders an icon, bakes one ItemDefinition per tier from the Tier Table "
                                    + $"and creates an Item_<Model> prefab (model + BoxCollider by the render "
                                            + $"bounds + Item), then fills the Prop Set. Existing icons are kept — "
                                                    + $"drop your own PNG named "
                                                    + $"Item_<Model>.png into the Icons Folder to override. "
                                                            + $"Force Icons re-renders all of them.",
                MessageType.Info);

            _modelsFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Models Folder", _modelsFolder, typeof(DefaultAsset), false);
            _prefabsFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Prefabs Folder", _prefabsFolder, typeof(DefaultAsset), false);
            _definitionsFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Definitions Folder", _definitionsFolder, typeof(DefaultAsset), false);
            _iconsFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Icons Folder", _iconsFolder, typeof(DefaultAsset), false);
            _propSet = (PropSet)EditorGUILayout.ObjectField("Prop Set", _propSet, typeof(PropSet), false);
            _tierTable = (TierTable)EditorGUILayout.ObjectField("Tier Table", _tierTable, typeof(TierTable), false);
            _ghostMaterial = (Material)EditorGUILayout.ObjectField(
                "Ghost Material", _ghostMaterial, typeof(Material), false);
            _ghostFadeConfig = (GhostFadeConfig)EditorGUILayout.ObjectField(
                "Ghost Fade Config", _ghostFadeConfig, typeof(GhostFadeConfig), false);
            _forceIcons = EditorGUILayout.Toggle("Force Icons", _forceIcons);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate"))
            {
                Generate();
            }
        }

        private void LoadState()
        {
            if (EditorPrefs.HasKey(PrefsKey) == false)
            {
                return;
            }

            WindowState state = JsonUtility.FromJson<WindowState>(EditorPrefs.GetString(PrefsKey));

            if (state == null)
            {
                return;
            }

            _modelsFolder = LoadStateAsset<DefaultAsset>(state.ModelsPath);
            _prefabsFolder = LoadStateAsset<DefaultAsset>(state.PrefabsPath);
            _definitionsFolder = LoadStateAsset<DefaultAsset>(state.DefinitionsPath);
            _iconsFolder = LoadStateAsset<DefaultAsset>(state.IconsPath);
            _propSet = LoadStateAsset<PropSet>(state.PropSetPath);
            _tierTable = LoadStateAsset<TierTable>(state.TierTablePath);
            _ghostMaterial = LoadStateAsset<Material>(state.GhostMaterialPath);
            _ghostFadeConfig = LoadStateAsset<GhostFadeConfig>(state.GhostFadeConfigPath);
            _forceIcons = state.ForceIcons;
        }

        private void SaveState()
        {
            WindowState state = new WindowState();
            state.ModelsPath = GetAssetPathOrNull(_modelsFolder);
            state.PrefabsPath = GetAssetPathOrNull(_prefabsFolder);
            state.DefinitionsPath = GetAssetPathOrNull(_definitionsFolder);
            state.IconsPath = GetAssetPathOrNull(_iconsFolder);
            state.PropSetPath = GetAssetPathOrNull(_propSet);
            state.TierTablePath = GetAssetPathOrNull(_tierTable);
            state.GhostMaterialPath = GetAssetPathOrNull(_ghostMaterial);
            state.GhostFadeConfigPath = GetAssetPathOrNull(_ghostFadeConfig);
            state.ForceIcons = _forceIcons;

            EditorPrefs.SetString(PrefsKey, JsonUtility.ToJson(state));
        }

        private static T LoadStateAsset<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path) == true)
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static string GetAssetPathOrNull(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                return string.Empty;
            }

            return AssetDatabase.GetAssetPath(asset);
        }

        [Serializable]
        private class WindowState
        {
            public string ModelsPath;
            public string PrefabsPath;
            public string DefinitionsPath;
            public string IconsPath;
            public string PropSetPath;
            public string TierTablePath;
            public string GhostMaterialPath;
            public string GhostFadeConfigPath;
            public bool ForceIcons;
        }
    }
}
