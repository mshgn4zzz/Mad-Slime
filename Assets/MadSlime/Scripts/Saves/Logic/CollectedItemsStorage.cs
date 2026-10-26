using System;
using YG;
namespace Saves
{
    public class CollectedItemsStorage : ICollectedItemsStorage
    {
        private SavesCore _core;
        private bool _isCountDirty;

        public CollectedItemsStorage(SavesCore core)
        {
            if (core == null)
            {
                throw new ArgumentNullException(nameof(core),
                    "CollectedItemsStorage requires a SavesCore instance.");
            }

            _core = core;
        }

        public int CollectedItemsCount => _core.LoadData().CollectedItemsCount;

        public void RegisterCollectedItem()
        {
            SavesYG data = _core.LoadData();
            data.CollectedItemsCount = checked(data.CollectedItemsCount + 1);
            _isCountDirty = true;
        }

        public void CommitCollectedItems()
        {
            if (_isCountDirty == false)
            {
                return;
            }

            _core.SaveChanges();
            _isCountDirty = false;
        }

        public void ResetCountDirtyFlag()
        {
            _isCountDirty = false;
        }
    }
}
