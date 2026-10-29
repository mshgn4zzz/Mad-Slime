using System;
using System.Collections.Generic;
using MadSlime.Items.Data;
using MadSlime.Items.View;
using UnityEditor;
using UnityEngine;

namespace MadSlime.Items.Editor
{
    public partial class ItemPropFactory
    {
        private void BakeDefinitions(List<ItemPropJob> jobs, string definitionsPath)
        {
            IReadOnlyList<TierEntry> entries = _tierTable.Entries;

            for (int i = 0; i < jobs.Count; i++)
            {
                int jobNumber = i + 1;
                float progress = (float)i / jobs.Count;

                EditorUtility.DisplayProgressBar("Prop Factory",
                    $"Definitions {jobs[i].PrefabName} ({jobNumber}/{jobs.Count})", progress);
                BakeJobDefinitions(jobs[i], entries, definitionsPath);
            }
        }

        private void BakeJobDefinitions(ItemPropJob job, IReadOnlyList<TierEntry> entries, string definitionsPath)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                TierEntry entry = entries[i];
                string assetPath = $"{definitionsPath}/D_{job.PrefabName}_{entry.Tier}.asset";
                ItemDefinition definition = AssetDatabase.LoadAssetAtPath<ItemDefinition>(assetPath);

                if (definition == null)
                {
                    definition = CreateInstance<ItemDefinition>();
                    AssetDatabase.CreateAsset(definition, assetPath);
                }

                SerializedObject serialized = new SerializedObject(definition);

                serialized.FindProperty(TierField).intValue = (int)entry.Tier;
                serialized.FindProperty(IconField).objectReferenceValue = LoadIcon(job.IconPath, job.PrefabName);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                job.AddDefinition(definition);

                if (entry.Tier == SizeTier.Small)
                {
                    job.SetFallbackDefinition(definition);
                }
            }

            if (job.FallbackDefinition == null)
            {
                job.SetFallbackDefinition(job.Definitions[0]);
            }
        }

        private Sprite LoadIcon(string iconPath, string prefabName)
        {
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

            return icon;
        }

        private void CreateItemPrefabs(List<ItemPropJob> jobs, int collectableLayer)
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                int jobNumber = i + 1;
                float progress = (float)i / jobs.Count;

                EditorUtility.DisplayProgressBar("Prop Factory",
                    $"Prefab {jobs[i].PrefabName} ({jobNumber}/{jobs.Count})", progress);

                if (AssetDatabase.LoadAssetAtPath<GameObject>(jobs[i].PrefabPath) != null)
                {
                    UpdateItemPrefab(jobs[i], collectableLayer);
                }
                else
                {
                    CreateItemPrefab(jobs[i], collectableLayer);
                }

                Item prefab = AssetDatabase.LoadAssetAtPath<Item>(jobs[i].PrefabPath);

                jobs[i].SetPrefab(prefab);
            }
        }

        private void CreateItemPrefab(ItemPropJob job, int collectableLayer)
        {
            GameObject root = new GameObject(job.PrefabName)
            {
                layer = collectableLayer
            };

            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(job.Model, root.transform);

            modelInstance.transform.localPosition = Vector3.zero;

            BoxCollider collider = root.AddComponent<BoxCollider>();

            ApplyCollider(collider, ComputeBounds(root));
            ApplyNormalizedScale(root, collider);

            Item item = root.AddComponent<Item>();

            WriteItemFields(item, collider, job.FallbackDefinition);

            PrefabUtility.SaveAsPrefabAsset(root, job.PrefabPath);
            DestroyImmediate(root);
        }

        private void UpdateItemPrefab(ItemPropJob job, int collectableLayer)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(job.PrefabPath);

            try
            {
                contents.layer = collectableLayer;

                if (contents.TryGetComponent(out BoxCollider collider) == false)
                {
                    collider = contents.AddComponent<BoxCollider>();
                }

                ApplyCollider(collider, ComputeBounds(contents));
                ApplyNormalizedScale(contents, collider);

                if (contents.TryGetComponent(out Item item) == false)
                {
                    item = contents.AddComponent<Item>();
                }

                WriteItemFields(item, collider, job.FallbackDefinition);

                PrefabUtility.SaveAsPrefabAsset(contents, job.PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private void WriteItemFields(Item item, BoxCollider collider, ItemDefinition fallbackDefinition)
        {
            SerializedObject serialized = new SerializedObject(item);

            serialized.FindProperty(DefinitionField).objectReferenceValue = fallbackDefinition;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (item.TryGetComponent(out ItemBody body) == false)
            {
                body = item.gameObject.AddComponent<ItemBody>();
            }

            SerializedObject serializedBody = new SerializedObject(body);

            serializedBody.FindProperty(ColliderField).objectReferenceValue = collider;
            serializedBody.ApplyModifiedPropertiesWithoutUndo();

            if (item.TryGetComponent(out GhostAnimator animator) == false)
            {
                animator = item.gameObject.AddComponent<GhostAnimator>();
            }

            SerializedObject serializedVisual = new SerializedObject(animator);

            serializedVisual.FindProperty(GhostMaterialField).objectReferenceValue = _ghostMaterial;
            serializedVisual.FindProperty(GhostFadeConfigField).objectReferenceValue = _ghostFadeConfig;
            serializedVisual.ApplyModifiedPropertiesWithoutUndo();
        }

        private void WritePropSet(List<ItemPropJob> jobs)
        {
            SerializedObject serialized = new SerializedObject(_propSet);

            SerializedProperty props = serialized.FindProperty(PropsField);
            SerializedProperty variants = serialized.FindProperty(VariantsField);

            if (props == null || props.isArray == false || variants == null || variants.isArray == false)
            {
                throw new InvalidOperationException(
                    $"[PropFactory] PropSet '{_propSet.name}' has no {PropsField} or {VariantsField} list.");
            }

            props.ClearArray();

            for (int i = 0; i < jobs.Count; i++)
            {
                props.InsertArrayElementAtIndex(props.arraySize);
                props.GetArrayElementAtIndex(props.arraySize - 1).objectReferenceValue = jobs[i].Prefab;
            }

            variants.ClearArray();

            for (int i = 0; i < jobs.Count; i++)
            {
                for (int j = 0; j < jobs[i].Definitions.Count; j++)
                {
                    variants.InsertArrayElementAtIndex(variants.arraySize);

                    SerializedProperty element = variants.GetArrayElementAtIndex(variants.arraySize - 1);

                    element.FindPropertyRelative(PrefabField).objectReferenceValue = jobs[i].Prefab;
                    element.FindPropertyRelative(DefinitionField).objectReferenceValue = jobs[i].Definitions[j];
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private void ApplyNormalizedScale(GameObject root, BoxCollider collider)
        {
            float largestDimension = Mathf.Max(collider.size.x, collider.size.y, collider.size.z);

            root.transform.localScale = Vector3.one * (NormalizedModelSize / largestDimension);

            Vector3 pivotOffset = collider.center;

            pivotOffset.y -= collider.size.y * 0.5f;

            for (int childIndex = 0; childIndex < root.transform.childCount; childIndex++)
            {
                root.transform.GetChild(childIndex).localPosition -= pivotOffset;
            }

            collider.center -= pivotOffset;
        }

        private void ApplyCollider(BoxCollider collider, Bounds bounds)
        {
            collider.size = bounds.size;
            collider.center = bounds.center;
        }

        private Bounds ComputeBounds(GameObject instance)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            Bounds world = new Bounds(instance.transform.position, Vector3.zero);

            for (int i = 0; i < renderers.Length; i++)
            {
                world.Encapsulate(renderers[i].bounds);
            }

            Matrix4x4 worldToLocal = instance.transform.worldToLocalMatrix;

            Vector3 localSize = new Vector3
            (
                Mathf.Abs(worldToLocal.m00) * world.size.x
                + Mathf.Abs(worldToLocal.m01) * world.size.y
                + Mathf.Abs(worldToLocal.m02) * world.size.z,
                Mathf.Abs(worldToLocal.m10) * world.size.x
                + Mathf.Abs(worldToLocal.m11) * world.size.y
                + Mathf.Abs(worldToLocal.m12) * world.size.z,
                Mathf.Abs(worldToLocal.m20) * world.size.x
                + Mathf.Abs(worldToLocal.m21) * world.size.y
                + Mathf.Abs(worldToLocal.m22) * world.size.z
            );

            if (localSize.x <= 0f || localSize.y <= 0f || localSize.z <= 0f)
            {
                throw new InvalidOperationException(
                    $"[PropFactory] '{instance.name}': computed a degenerate collider size {localSize} " +
                    "from world size " + world.size + ". Check the model's scale in its hierarchy.");
            }

            return new Bounds(instance.transform.InverseTransformPoint(world.center), localSize);
        }
    }
}
