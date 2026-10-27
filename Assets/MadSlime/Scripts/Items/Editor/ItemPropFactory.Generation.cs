using System.Collections.Generic;
using System.IO;
using Items;
using Levels;
using UnityEditor;
using UnityEngine;

namespace Items.Editor
{
    public partial class ItemPropFactory
    {
        private void Generate()
        {
            if (Validate() == false)
            {
                return;
            }

            string modelsPath = ItemFactoryTools.GetFolderPath(_modelsFolder);
            string prefabsPath = ItemFactoryTools.GetFolderPath(_prefabsFolder);
            string definitionsPath = ItemFactoryTools.GetFolderPath(_definitionsFolder);
            string iconsPath = ItemFactoryTools.GetFolderPath(_iconsFolder);

            List<ItemPropJob> jobs = CollectModelJobs(modelsPath, prefabsPath, iconsPath);

            if (jobs == null)
            {
                return;
            }

            int collectableLayer = LayerMask.NameToLayer("Collectable");

            if (collectableLayer < 0)
            {
                                Debug.LogError("[PropFactory] Layer 'Collectable' does not exist. Add it in Project "
                                        + "Settings → Tags and Layers.");
                return;
            }

            _iconLayer = ItemFactoryTools.ResolveFreeLayer();

            if (_iconLayer < 0)
            {
                Debug.LogError("[PropFactory] No unnamed layer left in 9-31 for icon rendering. Free one in "
                        + "Project Settings → Tags and Layers.");
                return;
            }

            ItemFactoryTools.EnsureFolder(prefabsPath);
            ItemFactoryTools.EnsureFolder(definitionsPath);
            ItemFactoryTools.EnsureFolder(iconsPath);

            int renderedIcons;

            try
            {
                renderedIcons = RenderIcons(jobs);

                if (renderedIcons > 0)
                {
                    AssetDatabase.Refresh();
                }

                for (int i = 0; i < jobs.Count; i++)
                {
                    ItemFactoryTools.EnsureSpriteImport(jobs[i].IconPath);
                }

                BakeDefinitions(jobs, definitionsPath);
                CreateItemPrefabs(jobs, collectableLayer);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            WritePropSet(jobs);

            AssetDatabase.SaveAssetIfDirty(_propSet);

            int variantsCount = jobs.Count * _tierTable.Entries.Count;

            Debug.Log(
                                $"[PropFactory] Done: {jobs.Count} props, {renderedIcons} icons rendered "
                                        + $"({jobs.Count - renderedIcons} kept),  " +
                $"{variantsCount} definitions, {variantsCount} variants into PropSet '{_propSet.name}'.");
        }

        private bool Validate()
        {
            List<string> missing = new List<string>();

            if (_modelsFolder == null)
            {
                missing.Add("Models Folder");
            }

            if (_prefabsFolder == null)
            {
                missing.Add("Prefabs Folder");
            }

            if (_definitionsFolder == null)
            {
                missing.Add("Definitions Folder");
            }

            if (_iconsFolder == null)
            {
                missing.Add("Icons Folder");
            }

            if (_propSet == null)
            {
                missing.Add("Prop Set");
            }

            if (_tierTable == null)
            {
                missing.Add("Tier Table");
            }

            if (_ghostMaterial == null)
            {
                missing.Add("Ghost Material");
            }

            if (_ghostFadeConfig == null)
            {
                missing.Add("Ghost Fade Config");
            }

            if (missing.Count > 0)
            {
                Debug.LogError("[PropFactory] Not assigned: " + string.Join(", ", missing));
                return false;
            }

            if (_tierTable.Entries.Count == 0)
            {
                Debug.LogError($"[PropFactory] Tier Table '{_tierTable.name}' has no entries.");
                return false;
            }

            return true;
        }

        private List<ItemPropJob> CollectModelJobs(string modelsPath, string prefabsPath, string iconsPath)
        {
            string[] modelGuids = AssetDatabase.FindAssets("t:Prefab", new[] { modelsPath });

            if (modelGuids.Length == 0)
            {
                Debug.LogError($"[PropFactory] No model prefabs found in '{modelsPath}'.");
                return null;
            }

            List<ItemPropJob> jobs = new List<ItemPropJob>(modelGuids.Length);
            Dictionary<string, string> nameToPath = new Dictionary<string, string>();

            for (int i = 0; i < modelGuids.Length; i++)
            {
                string modelPath = AssetDatabase.GUIDToAssetPath(modelGuids[i]);
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

                if (model == null)
                {
                    continue;
                }

                if (model.TryGetComponent<Item>(out Item existingItem) == true)
                {
                    continue;
                }

                if (nameToPath.ContainsKey(model.name) == true)
                {
                    Debug.LogError(
                                                $"[PropFactory] Duplicate model name '{model.name}': "
                                                        + $"'{nameToPath[model.name]}' and '{modelPath}'.  " +
                        "Model names must be unique — they define prefab, definition and icon names.");
                    return null;
                }

                if (model.GetComponentsInChildren<Renderer>().Length == 0)
                {
                    Debug.LogError(
                                                $"[PropFactory] Model '{modelPath}' has no Renderers. Item.Awake "
                                                        + $"would fail at runtime for such a prefab.");
                    return null;
                }

                nameToPath.Add(model.name, modelPath);

                ItemPropJob job = new ItemPropJob(model, prefabsPath, iconsPath);

                jobs.Add(job);
            }

            if (jobs.Count == 0)
            {
                Debug.LogError($"[PropFactory] No model prefabs found in '{modelsPath}'.");
                return null;
            }

            jobs.Sort(CompareJobNames);

            return jobs;
        }

        private static int CompareJobNames(ItemPropJob left, ItemPropJob right)
        {
            return string.CompareOrdinal(left.PrefabName, right.PrefabName);
        }
    }
}
