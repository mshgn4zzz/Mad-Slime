using System;
using System.IO;
using Items.Editor;
using Shop;
using Skins;
using UnityEditor;
using UnityEngine;

namespace Skins.Editor
{
    public class SkinItemFactory : EditorWindow
    {
        private const string PrefsKey = "MadSlime.SkinItemFactory";
        private const int IconSize = 512;
        private const float IconCameraDistance = 10f;
        private const string DefaultSkinsPath = "Assets/MadSlime/Scriptables/Skins";
        private const string DefaultPrefabsPath = "Assets/MadSlime/Resources/PlayerModel/Prefabs";
        private const string DefaultIconsPath = "Assets/MadSlime/Resources/Icons/Skins";
        private const string DefaultShopContentPath = "Assets/MadSlime/Scriptables/Shop/ShopContent.asset";

        private const string ModelField = "_model";
        private const string IconField = "_icon";
        private const string RarityField = "_rarity";
        private const string IdField = "_id";
        private const string SkinItemsField = "_skinItems";

        private GameObject _sourceModel;
        private ShopContent _shopContent;
        private DefaultAsset _skinsFolder;
        private DefaultAsset _prefabsFolder;
        private DefaultAsset _iconsFolder;
        private string _skinId = "";
        private SkinRarity _rarity = SkinRarity.Common;
        private bool _forceIcon;
        private bool _replaceExisting;

        public static void CreateFromCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();

            string Argument(string key)
            {
                int index = Array.IndexOf(args, key);

                if (index < 0 || index + 1 >= args.Length || args[index + 1].StartsWith("-"))
                {
                    throw new ArgumentException($"Skin Factory requires {key}.");
                }

                return args[index + 1];
            }

            string path = Argument("-skinSource");
            string id = Argument("-skinId");

            if (Enum.TryParse(Argument("-skinRarity"), true, out SkinRarity rarity) == false
                || Enum.IsDefined(typeof(SkinRarity), rarity) == false)
            {
                throw new ArgumentException("Unknown skin rarity.");
            }

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (source == null)
            {
                throw new ArgumentException($"Skin model does not exist: {path}");
            }

            RebuildSkin(source, id, rarity);
        }

        public static SkinItem RebuildSkin(GameObject sourceModel, string skinId, SkinRarity rarity)
        {
            ItemFactoryTools.EnsureFolder(DefaultPrefabsPath);
            ItemFactoryTools.EnsureFolder(DefaultSkinsPath);
            ItemFactoryTools.EnsureFolder(DefaultIconsPath);

            SkinItemFactory factory = CreateInstance<SkinItemFactory>();

            try
            {
                factory._sourceModel = sourceModel;
                factory._skinId = skinId;
                factory._rarity = rarity;
                factory._shopContent = AssetDatabase.LoadAssetAtPath<ShopContent>(DefaultShopContentPath);
                factory._skinsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultSkinsPath);
                factory._prefabsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultPrefabsPath);
                factory._iconsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultIconsPath);
                factory._forceIcon = true;
                factory._replaceExisting = true;
                factory.CreateSkin();
                return AssetDatabase.LoadAssetAtPath<SkinItem>($"{DefaultSkinsPath}/{skinId}.asset");
            }
            finally
            {
                DestroyImmediate(factory);
            }
        }

        [MenuItem("Mad Slime/Skin Factory")]
        private static void Open()
        {
            GetWindow<SkinItemFactory>("Skin Factory");
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
                "Drop a model or a prefab into Source Model. A prefab is used as is; a raw model is saved as " +
                                "'<Name>Skin.prefab' into the Prefabs Folder (an existing file is reused, never "
                                        + "duplicated). The tool  " +
                "renders the icon into the Icons Folder as '<Id>.png' (kept unless Force Icon is on), creates the " +
                                "SkinItem asset in the Skins Folder and appends it to Shop Content. The id must be "
                                        + "unique — it is the  " +
                "save key of the skin and must never change after release.",
                MessageType.Info);

            _sourceModel = (GameObject)EditorGUILayout.ObjectField(
                "Source Model", _sourceModel, typeof(GameObject), false);
            _shopContent = (ShopContent)EditorGUILayout.ObjectField(
                "Shop Content", _shopContent, typeof(ShopContent), false);
            _skinsFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Skins Folder", _skinsFolder, typeof(DefaultAsset), false);
            _prefabsFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Prefabs Folder", _prefabsFolder, typeof(DefaultAsset), false);
            _iconsFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Icons Folder", _iconsFolder, typeof(DefaultAsset), false);

            EditorGUILayout.Space();

            _skinId = EditorGUILayout.TextField("Skin Id", _skinId);
            _rarity = (SkinRarity)EditorGUILayout.EnumPopup("Rarity", _rarity);
            _forceIcon = EditorGUILayout.Toggle("Force Icon", _forceIcon);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Skin"))
            {
                CreateSkin();
            }
        }

        private void CreateSkin()
        {
            if (Validate() == false)
            {
                return;
            }

            string skinsPath = ItemFactoryTools.GetFolderPath(_skinsFolder);
            string prefabsPath = ItemFactoryTools.GetFolderPath(_prefabsFolder);
            string iconsPath = ItemFactoryTools.GetFolderPath(_iconsFolder);
            string assetPath = $"{skinsPath}/{_skinId}.asset";

            if (IsIdUsed(_skinId) == true && _replaceExisting == false)
            {
                                Debug.LogError($"[SkinFactory] Skin id '{_skinId}' is already used in ShopContent "
                                        + $"'{_shopContent.name}'.");
                return;
            }

            if (File.Exists(assetPath) == true && _replaceExisting == false)
            {
                Debug.LogError($"[SkinFactory] SkinItem asset already exists: {assetPath}");
                return;
            }

            GameObject prefab = ResolveSkinPrefab(prefabsPath);

            if (prefab == null)
            {
                return;
            }

            EnsurePreviewModel(prefab);
            ItemFactoryTools.EnsureFolder(skinsPath);

            string iconPath = $"{iconsPath}/{_skinId}.png";

            RenderIconIfNeeded(prefab, iconPath);

            SkinItem skin = AssetDatabase.LoadAssetAtPath<SkinItem>(assetPath);
            bool isNew = skin == null;

            if (isNew)
            {
                skin = ScriptableObject.CreateInstance<SkinItem>();
            }

            SerializedObject serialized = new SerializedObject(skin);

            serialized.FindProperty(ModelField).objectReferenceValue = prefab;
            serialized.FindProperty(IconField).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            serialized.FindProperty(RarityField).intValue = (int)_rarity;
            serialized.FindProperty(IdField).stringValue = _skinId;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (isNew)
            {
                AssetDatabase.CreateAsset(skin, assetPath);
            }
            else
            {
                AssetDatabase.SaveAssetIfDirty(skin);
            }

            if (IsIdUsed(_skinId) == false)
            {
                AppendToShopContent(skin);
            }

            AssetDatabase.SaveAssetIfDirty(_shopContent);
            AssetDatabase.Refresh();

            Selection.activeObject = skin;
            EditorGUIUtility.PingObject(skin);

            Debug.Log($"[SkinFactory] Skin '{_skinId}' created at {assetPath} and appended to '{_shopContent.name}'.");
        }

        private bool Validate()
        {
            if (_sourceModel == null)
            {
                Debug.LogError("[SkinFactory] Source Model is not assigned.");
                return false;
            }

            if (_shopContent == null)
            {
                Debug.LogError("[SkinFactory] Shop Content is not assigned.");
                return false;
            }

            if (_skinsFolder == null || _prefabsFolder == null || _iconsFolder == null)
            {
                Debug.LogError("[SkinFactory] A folder is not assigned. Fill Skins, Prefabs and Icons folders.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_skinId) == true)
            {
                Debug.LogError("[SkinFactory] Skin Id is empty. It is the save key of the skin.");
                return false;
            }

            if (_sourceModel.GetComponentsInChildren<Renderer>().Length == 0)
            {
                                Debug.LogError($"[SkinFactory] '{_sourceModel.name}' has no Renderers — there is "
                                        + $"nothing to render the icon from.");
                return false;
            }

            return true;
        }

        private bool IsIdUsed(string skinId)
        {
            foreach (SkinItem item in _shopContent.SkinItems)
            {
                if (item != null && item.Id == skinId)
                {
                    return true;
                }
            }

            return false;
        }

        private GameObject ResolveSkinPrefab(string prefabsPath)
        {
            string sourcePath = AssetDatabase.GetAssetPath(_sourceModel);

            if (sourcePath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) == true)
            {
                return _sourceModel;
            }

            ItemFactoryTools.EnsureFolder(prefabsPath);

            string prefabPath = $"{prefabsPath}/{_sourceModel.name}Skin.prefab";

            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (existing != null && _replaceExisting == false)
            {
                return existing;
            }

            GameObject instance;

            if (string.IsNullOrEmpty(sourcePath) == true)
            {
                instance = UnityEngine.Object.Instantiate(_sourceModel);
            }
            else
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(_sourceModel);
            }

            if (_replaceExisting && PrefabUtility.IsPartOfPrefabInstance(instance))
            {
                PrefabUtility.UnpackPrefabInstance(
                    instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            instance.name = $"{_sourceModel.name}Skin";

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

            DestroyImmediate(instance);

            return prefab;
        }

        private void EnsurePreviewModel(GameObject prefab)
        {
            if (prefab.TryGetComponent(out SkinModel skinModel) == true)
            {
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                contents.AddComponent<SkinModel>();
                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private void RenderIconIfNeeded(GameObject prefab, string iconPath)
        {
            if (File.Exists(iconPath) == true && _forceIcon == false)
            {
                return;
            }

            ItemFactoryTools.EnsureFolder(Path.GetDirectoryName(iconPath)?.Replace('\\', '/'));

            int iconLayer = ItemFactoryTools.ResolveFreeLayer();

            if (iconLayer < 0)
            {
                Debug.LogError("[SkinFactory] No unnamed layer left in 9-31 for icon rendering. Free one in "
                        + "Project Settings → Tags and Layers.");
                return;
            }

            IconRenderSettings settings = new IconRenderSettings
            {
                LightEuler = new Vector3(35f, 210f, 0f),
                RelightToCamera = false,
                CameraOffset = Quaternion.Euler(0f, 30f, 0f) * new Vector3(0f, 0.35f, 1f),
                OrthographicPadding = 1f,
                FitByProjectiveBounds = false
            };
            Texture2D texture = ItemFactoryTools.RenderToTexture(prefab, iconLayer, IconSize, settings);

            File.WriteAllBytes(iconPath, texture.EncodeToPNG());
            DestroyImmediate(texture);

            AssetDatabase.ImportAsset(iconPath);

            TextureImporter importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;

            if (importer == null)
            {
                Debug.LogWarning($"[SkinFactory] No TextureImporter for '{iconPath}'.");
                return;
            }

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
        }

        private void AppendToShopContent(SkinItem skin)
        {
            SerializedObject serialized = new SerializedObject(_shopContent);
            SerializedProperty items = serialized.FindProperty(SkinItemsField);

            if (items == null || items.isArray == false)
            {
                throw new InvalidOperationException(
                    $"[SkinFactory] ShopContent '{_shopContent.name}' has no _skinItems list.");
            }

            items.InsertArrayElementAtIndex(items.arraySize);
            items.GetArrayElementAtIndex(items.arraySize - 1).objectReferenceValue = skin;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }







        private void LoadState()
        {
            if (EditorPrefs.HasKey(PrefsKey) == true)
            {
                WindowState state = JsonUtility.FromJson<WindowState>(EditorPrefs.GetString(PrefsKey));

                if (state != null)
                {
                    _shopContent = LoadStateAsset<ShopContent>(state.ShopContentPath);
                    _skinsFolder = LoadStateAsset<DefaultAsset>(state.SkinsPath);
                    _prefabsFolder = LoadStateAsset<DefaultAsset>(state.PrefabsPath);
                    _iconsFolder = LoadStateAsset<DefaultAsset>(state.IconsPath);
                    _rarity = state.Rarity;
                    _forceIcon = state.ForceIcon;

                    return;
                }
            }

            _shopContent = AssetDatabase.LoadAssetAtPath<ShopContent>(DefaultShopContentPath);
            _skinsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultSkinsPath);
            _prefabsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultPrefabsPath);
            _iconsFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(DefaultIconsPath);
        }

        private void SaveState()
        {
            WindowState state = new WindowState();

            state.ShopContentPath = GetAssetPathOrNull(_shopContent);
            state.SkinsPath = GetAssetPathOrNull(_skinsFolder);
            state.PrefabsPath = GetAssetPathOrNull(_prefabsFolder);
            state.IconsPath = GetAssetPathOrNull(_iconsFolder);
            state.Rarity = _rarity;
            state.ForceIcon = _forceIcon;

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
            public string ShopContentPath;
            public string SkinsPath;
            public string PrefabsPath;
            public string IconsPath;
            public SkinRarity Rarity;
            public bool ForceIcon;
        }
    }
}
