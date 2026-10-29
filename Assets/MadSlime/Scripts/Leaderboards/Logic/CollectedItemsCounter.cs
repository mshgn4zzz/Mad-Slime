using System;
using MadSlime.Collectables.Logic;
using MadSlime.Items.View;
using MadSlime.Saves.Interfaces;

namespace MadSlime.Leaderboards.Logic
{
    public class CollectedItemsCounter : IDisposable
    {
        private ItemCollector _collector;
        private ICollectedItemsStorage _storage;

        public CollectedItemsCounter(ItemCollector collector, ICollectedItemsStorage storage)
        {
            _collector = collector;
            _storage = storage;
            _collector.ItemCollected += OnItemCollected;
        }

        public void Dispose()
        {
            _collector.ItemCollected -= OnItemCollected;
            _storage.CommitCollectedItems();
        }

        public void CommitSession()
        {
            _storage.CommitCollectedItems();
        }

        private void OnItemCollected(Item item)
        {
            _storage.RegisterCollectedItem();
        }
    }
}
