using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Collectables.Logic;
using MadSlime.Items.Data;
using MadSlime.Items.Logic;
using MadSlime.Items.View;
using MadSlime.Quota.Data;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Levels.Logic
{
    public class LevelItemsSpawner
    {
        private const float SpawnFrameBudgetSeconds = 0.006f;

        private ItemFactory _itemFactory;
        private Transform _itemsRoot;
        private ItemCollector _collector;
        private List<Item> _activeItems = new List<Item>(256);
        private Dictionary<Item, int> _activeIndexes = new Dictionary<Item, int>(256);
        private Dictionary<ItemDefinition, int> _spawnedCounts = new Dictionary<ItemDefinition, int>();

        public LevelItemsSpawner(ItemFactory itemFactory, Transform itemsRoot)
        {
            ArgumentGuard.CheckNotNull(itemFactory, nameof(itemFactory));

            ArgumentGuard.CheckNotNull(itemsRoot, nameof(itemsRoot));

            _itemFactory = itemFactory;
            _itemsRoot = itemsRoot;
        }

        public IReadOnlyList<Item> ActiveItems => _activeItems;

        public IReadOnlyDictionary<ItemDefinition, int> SpawnedCounts => _spawnedCounts;

        public void Attach(ItemCollector collector)
        {
            ArgumentGuard.CheckNotNull(collector, nameof(collector));

            Detach();
            _collector = collector;
            _collector.ItemCollected += OnItemCollected;
        }

        public void Detach()
        {
            if (_collector == null)
            {
                return;
            }

            _collector.ItemCollected -= OnItemCollected;
            _collector = null;
        }

        public void Clear()
        {
            foreach (Item item in _activeItems)
            {
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }

            _activeItems.Clear();
            _activeIndexes.Clear();
            _spawnedCounts.Clear();
        }

        public async UniTask SpawnAsync(
            IReadOnlyList<LevelPlacement> placements,
            Action<float> progress,
            CancellationToken cancellationToken)
        {
            float frameStart = Time.realtimeSinceStartup;

            for (int i = 0; i < placements.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                LevelPlacement placement = placements[i];
                Item item = _itemFactory.Create(
                    placement.Prefab,
                    placement.Definition,
                    placement.Position,
                    placement.Scale,
                    _itemsRoot);

                _activeItems.Add(item);
                _activeIndexes.Add(item, _activeItems.Count - 1);
                CountSpawned(placement.Definition);

                if (Time.realtimeSinceStartup - frameStart >= SpawnFrameBudgetSeconds)
                {
                    float completed = (i + 1) / (float)placements.Count;

                    progress?.Invoke(completed);
                    await UniTask.NextFrame(cancellationToken: cancellationToken);
                    frameStart = Time.realtimeSinceStartup;
                }
            }

            progress?.Invoke(1f);
        }

        public void AssignQuotaLayers(IReadOnlyList<QuotaEntry> quota)
        {
            ArgumentGuard.CheckNotNull(quota, nameof(quota));

            int collectableLayer = LayerMask.NameToLayer("Collectable");
            int quotaLayer = LayerMask.NameToLayer("QuotaCollectable");

            if (collectableLayer < 0 || quotaLayer < 0)
            {
                throw new InvalidOperationException(
                    "LevelItemsSpawner requires Collectable and QuotaCollectable layers in Project Settings.");
            }

            HashSet<ItemDefinition> quotaTypes = new HashSet<ItemDefinition>();

            foreach (QuotaEntry entry in quota)
            {
                quotaTypes.Add(entry.Definition);
            }

            foreach (Item item in _activeItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.TryGetComponent(out ItemBody body) == false || body.Collider == null)
                {
                    throw new InvalidOperationException($"{item.name}: an ItemBody with a collider is required.");
                }

                if (quotaTypes.Contains(item.Definition))
                {
                    body.Collider.gameObject.layer = quotaLayer;
                }
                else
                {
                    body.Collider.gameObject.layer = collectableLayer;
                }
            }
        }

        private void CountSpawned(ItemDefinition definition)
        {
            if (_spawnedCounts.TryGetValue(definition, out int count))
            {
                _spawnedCounts[definition] = count + 1;
                return;
            }

            _spawnedCounts[definition] = 1;
        }

        private void OnItemCollected(Item item)
        {
            int index = _activeIndexes[item];
            Item last = _activeItems[_activeItems.Count - 1];

            _activeItems[index] = last;
            _activeIndexes[last] = index;
            _activeItems.RemoveAt(_activeItems.Count - 1);
            _activeIndexes.Remove(item);
        }
    }
}
