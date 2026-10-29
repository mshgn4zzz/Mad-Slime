using System;
using System.IO;
using MadSlime.Items.Editor;
using MadSlime.Shop.Data;
using MadSlime.Shop.View;
using MadSlime.Skins.Data;
using UnityEditor;
using UnityEngine;

namespace MadSlime.Skins.Editor
{
    public class SkinItemFactory : EditorWindow
    {
        private const string PrefsKey = "MadSlime.SkinItemFactory";
        private const int IconSize = 512;
        private const string DefaultSkinsPath = "Assets/MadSlime/Scriptables/Skins";
        private const string DefaultPrefabsPath = "Assets/MadSlime/Resources/PlayerModel/Prefabs";
        private const string DefaultIconsPath = "Assets/MadSlime/Resources/Icons/Skins";
        private const string DefaultShopContentPath = "Assets/MadSlime/Scriptables/Shop/ShopContent.asset";

        private const string ModelField = "_model";
        private const string IconField = "_icon";
        private const string RarityField = "_rarity";
        private const string IdField = "_id";
        private const string SkinItemsField = "_skinItems";
        private ItemFactoryTools _tools = new ItemFactoryTools();

        private GameObject _sourceModel;
        private ShopContent _shopContent;
        private DefaultAsset _skinsFolder;
        private DefaultAsset _prefabsFolder;
        private DefaultAsset _iconsFolder;
        private string _skinId = "";
        private SkinRarity _rarity = SkinRarity.Common;
        private bool _isIconRegenerationForced;
        private bool _isReplacementEnabled;

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
            _isIconRegenerationForced = EditorGUILayout.Toggle("Force Icon", _isIconRegenerationForced);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Skin"))
            {
                CreateSkin();
            }
        }

        private void CreateSkin()
        {
            Validate();

            string skinsPath = _tools.GetFolderPath(_skinsFolder);
            string prefabsPath = _tools.GetFolderPath(_prefabsFolder);
            string iconsPath = _tools.GetFolderPath(_iconsFolder);
            string assetPath = $"{skinsPath}/{_skinId}.asset";

            if (IsIdUsed(_skinId) && _isReplacementEnabled == false)
            {
                throw new InvalidOperationException($"[SkinFactory] Skin id '{_skinId}' is already used in ShopContent "
                    + $"'{_shopContent.name}'.");
            }

            if (File.Exists(assetPath) && _isReplacementEnabled == false)
            {
                throw new InvalidOperationException($"[SkinFactory] SkinItem asset already exists: {assetPath}");
            }

            GameObject prefab = ResolveSkinPrefab(prefabsPath);

            if (prefab == null)
            {
                return;
            }

            EnsurePreviewModel(prefab);
            _tools.EnsureFolder(skinsPath);

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
        }

        private void Validate()
        {
            if (_sourceModel == null)
            {
                throw new InvalidOperationException("[SkinFactory] Source Model is not assigned.");
            }

            if (_shopContent == null)
            {
                throw new InvalidOperationException("[SkinFactory] Shop Content is not assigned.");
            }

            if (_skinsFolder == null || _prefabsFolder == null || _iconsFolder == null)
            {
                throw new InvalidOperationException(
                    "[SkinFactory] A folder is not assigned. Fill Skins, Prefabs and Icons folders.");
            }

            if (string.IsNullOrWhiteSpace(_skinId))
            {
                throw new InvalidOperationException("[SkinFactory] Skin Id is empty. It is the save key of the skin.");
            }

            if (_sourceModel.GetComponentsInChildren<Renderer>().Length == 0)
            {
                throw new InvalidOperationException($"[SkinFactory] '{_sourceModel.name}' has no Renderers — there is "
                    + $"nothing to render the icon from.");
            }
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

            if (sourcePath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                return _sourceModel;
            }

            _tools.EnsureFolder(prefabsPath);

            string prefabPath = $"{prefabsPath}/{_sourceModel.name}Skin.prefab";

            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (existing != null && _isReplacementEnabled == false)
            {
                return existing;
            }

            GameObject instance;

            if (string.IsNullOrEmpty(sourcePath))
            {
                instance = UnityEngine.Object.Instantiate(_sourceModel);
            }
            else
            {
                instance = (GameObject)PrefabUtility.InstantiatePrefab(_sourceModel);
            }

            if (_isReplacementEnabled && PrefabUtility.IsPartOfPrefabInstance(instance))
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
            if (prefab.TryGetComponent(out SkinModel skinModel))
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
            if (File.Exists(iconPath) && _isIconRegenerationForced == false)
            {
                return;
            }

            _tools.EnsureFolder(Path.GetDirectoryName(iconPath)?.Replace('\\', '/'));

            int iconLayer = _tools.ResolveFreeLayer();

            if (iconLayer < 0)
            {
                throw new InvalidOperationException(
                    "[SkinFactory] No unnamed layer left in 9-31 for icon rendering. Free one in "
                    + "Project Settings → Tags and Layers.");
            }

            IconRenderSettings settings = new IconRenderSettings
            {
                LightEuler = new Vector3(35f, 210f, 0f),
                IsRelightingEnabled = false,
                CameraOffset = Quaternion.Euler(0f, 30f, 0f) * new Vector3(0f, 0.35f, 1f),
                OrthographicPadding = 1f,
                IsProjectiveFitEnabled = false
            };
            Texture2D texture = _tools.RenderToTexture(prefab, iconLayer, IconSize, settings);

            File.WriteAllBytes(iconPath, texture.EncodeToPNG());
            DestroyImmediate(texture);

            AssetDatabase.ImportAsset(iconPath);

            TextureImporter importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;

            if (importer == null)
            {
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
            if (EditorPrefs.HasKey(PrefsKey))
            {
                SkinFactoryWindowState state = JsonUtility.FromJson<SkinFactoryWindowState>(
                EditorPrefs.GetString(PrefsKey));

                if (state != null)
                {
                    _shopContent = LoadStateAsset<ShopContent>(state.ShopContentPath);
                    _skinsFolder = LoadStateAsset<DefaultAsset>(state.SkinsPath);
                    _prefabsFolder = LoadStateAsset<DefaultAsset>(state.PrefabsPath);
                    _iconsFolder = LoadStateAsset<DefaultAsset>(state.IconsPath);
                    _rarity = state.Rarity;
                    _isIconRegenerationForced = state.IsIconRegenerationForced;

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
            SkinFactoryWindowState state = new SkinFactoryWindowState();

            state.ShopContentPath = GetAssetPathOrNull(_shopContent);
            state.SkinsPath = GetAssetPathOrNull(_skinsFolder);
            state.PrefabsPath = GetAssetPathOrNull(_prefabsFolder);
            state.IconsPath = GetAssetPathOrNull(_iconsFolder);
            state.Rarity = _rarity;
            state.IsIconRegenerationForced = _isIconRegenerationForced;

            EditorPrefs.SetString(PrefsKey, JsonUtility.ToJson(state));
        }

        private T LoadStateAsset<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private string GetAssetPathOrNull(UnityEngine.Object asset)
        {
            if (asset == null)
            {
                return string.Empty;
            }

            return AssetDatabase.GetAssetPath(asset);
        }
    }
}
