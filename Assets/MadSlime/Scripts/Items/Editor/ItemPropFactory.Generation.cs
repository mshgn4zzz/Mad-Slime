using System;
using System.Collections.Generic;
using MadSlime.Items.View;
using UnityEditor;
using UnityEngine;

namespace MadSlime.Items.Editor
{
    public partial class ItemPropFactory
    {
        private void Generate()
        {
            Validate();

            string modelsPath = _tools.GetFolderPath(_modelsFolder);
            string prefabsPath = _tools.GetFolderPath(_prefabsFolder);
            string definitionsPath = _tools.GetFolderPath(_definitionsFolder);
            string iconsPath = _tools.GetFolderPath(_iconsFolder);

            List<ItemPropJob> jobs = CollectModelJobs(modelsPath, prefabsPath, iconsPath);

            if (jobs == null)
            {
                return;
            }

            int collectableLayer = LayerMask.NameToLayer("Collectable");

            if (collectableLayer < 0)
            {
                throw new InvalidOperationException(
                    "[PropFactory] Layer 'Collectable' does not exist. Add it in Project "
                    + "Settings → Tags and Layers.");
            }

            _iconLayer = _tools.ResolveFreeLayer();

            if (_iconLayer < 0)
            {
                throw new InvalidOperationException(
                    "[PropFactory] No unnamed layer left in 9-31 for icon rendering. Free one in "
                    + "Project Settings → Tags and Layers.");
            }

            _tools.EnsureFolder(prefabsPath);
            _tools.EnsureFolder(definitionsPath);
            _tools.EnsureFolder(iconsPath);

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
                    _tools.EnsureSpriteImport(jobs[i].IconPath);
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
        }

        private void Validate()
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
                throw new InvalidOperationException("[PropFactory] Not assigned: " + string.Join(", ", missing));
            }

            if (_tierTable.Entries.Count == 0)
            {
                throw new InvalidOperationException($"[PropFactory] Tier Table '{_tierTable.name}' has no entries.");
            }
        }

        private List<ItemPropJob> CollectModelJobs(string modelsPath, string prefabsPath, string iconsPath)
        {
            string[] modelGuids = AssetDatabase.FindAssets("t:Prefab", new[] { modelsPath });

            if (modelGuids.Length == 0)
            {
                throw new InvalidOperationException($"[PropFactory] No model prefabs found in '{modelsPath}'.");
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

                if (model.TryGetComponent<Item>(out Item existingItem))
                {
                    continue;
                }

                if (nameToPath.ContainsKey(model.name))
                {
                    throw new InvalidOperationException(
                        $"[PropFactory] Duplicate model name '{model.name}': "
                        + $"'{nameToPath[model.name]}' and '{modelPath}'.  " +
                        "Model names must be unique — they define prefab, definition and icon names.");
                }

                if (model.GetComponentsInChildren<Renderer>().Length == 0)
                {
                    throw new InvalidOperationException(
                        $"[PropFactory] Model '{modelPath}' has no Renderers. Item.Awake "
                        + $"would fail at runtime for such a prefab.");
                }

                nameToPath.Add(model.name, modelPath);

                ItemPropJob job = new ItemPropJob(model, prefabsPath, iconsPath);

                jobs.Add(job);
            }

            if (jobs.Count == 0)
            {
                throw new InvalidOperationException($"[PropFactory] No model prefabs found in '{modelsPath}'.");
            }

            jobs.Sort(CompareJobNames);

            return jobs;
        }

        private int CompareJobNames(ItemPropJob left, ItemPropJob right)
        {
            return string.CompareOrdinal(left.PrefabName, right.PrefabName);
        }
    }
}
