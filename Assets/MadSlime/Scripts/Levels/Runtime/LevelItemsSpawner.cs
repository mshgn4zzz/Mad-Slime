using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Collectables;
using Items;
using UnityEngine;

namespace Game
{
    public class LevelItemsSpawner
    {
        private const float SpawnFrameBudgetSeconds = 0.006f;

        private ItemFactory _itemFactory;
        private Transform _itemsRoot;
        private ItemCollector _collector;
        private List<Item> _activeItems = new List<Item>(256);
        private Dictionary<ItemDefinition, int> _spawnedCounts = new Dictionary<ItemDefinition, int>();

        public IReadOnlyList<Item> ActiveItems => _activeItems;
        public Dictionary<ItemDefinition, int> SpawnedCounts => _spawnedCounts;

        public LevelItemsSpawner(ItemFactory itemFactory, Transform itemsRoot)
        {
            if (itemFactory == null)
            {
                throw new ArgumentNullException(nameof(itemFactory),
                    "LevelItemsSpawner requires an ItemFactory.");
            }

            if (itemsRoot == null)
            {
                throw new ArgumentNullException(nameof(itemsRoot),
                    "LevelItemsSpawner requires an items root transform.");
            }

            _itemFactory = itemFactory;
            _itemsRoot = itemsRoot;
        }

        public static float GetRadiusXZ(Item prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab),
                    "LevelItemsSpawner: Item prefab is not assigned.");
            }

            if (prefab.TryGetComponent(out ItemBody body) == false
                || body.Collider is BoxCollider boxCollider == false)
            {
                throw new InvalidOperationException(
                    $"{prefab.name}: Item prefab needs a BoxCollider in the Collider field on ItemBody to measure the XZ radius.");
            }

            Vector3 scaledSize = Vector3.Scale(boxCollider.size, prefab.transform.lossyScale);

            Vector3 scaledCenter = Vector3.Scale(boxCollider.center, prefab.transform.lossyScale);
            float footprintRadius = new Vector2(scaledSize.x, scaledSize.z).magnitude * 0.5f;
            float pivotOffset = new Vector2(scaledCenter.x, scaledCenter.z).magnitude;

            return footprintRadius + pivotOffset;
        }

        public void Attach(ItemCollector collector)
        {
            if (collector == null)
            {
                throw new ArgumentNullException(nameof(collector));
            }

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
            _spawnedCounts.Clear();
        }

        public async UniTask SpawnAsync(IReadOnlyList<LevelPlacement> placements, Action<float> progress,
            CancellationToken cancellationToken)
        {
            float frameStart = Time.realtimeSinceStartup;

            for (int i = 0; i < placements.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                LevelPlacement placement = placements[i];
                Item item = _itemFactory.Create(placement.Prefab, placement.Definition,
                    placement.Position, placement.Scale, _itemsRoot);
                _activeItems.Add(item);
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

        private void OnItemCollected(Item item)
        {
            _activeItems.Remove(item);
        }

        private void CountSpawned(ItemDefinition definition)
        {
            if (_spawnedCounts.TryGetValue(definition, out int count) == true)
            {
                _spawnedCounts[definition] = count + 1;
                return;
            }

            _spawnedCounts[definition] = 1;
        }
    }
}
