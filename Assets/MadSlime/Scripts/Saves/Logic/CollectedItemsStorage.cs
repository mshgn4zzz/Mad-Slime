using MadSlime.Saves.Interfaces;
using MadSlime.Validation;
using YG;

namespace MadSlime.Saves.Logic
{
    public class CollectedItemsStorage : ICollectedItemsStorage
    {
        private SavesCore _core;
        private bool _isCountSavePending;

        public CollectedItemsStorage(SavesCore core)
        {
            ArgumentGuard.CheckNotNull(core, nameof(core));

            _core = core;
        }

        public int CollectedItemsCount => _core.LoadData().CollectedItemsCount;

        public void RegisterCollectedItem()
        {
            SavesYG data = _core.LoadData();

            data.CollectedItemsCount = checked(data.CollectedItemsCount + 1);
            _isCountSavePending = true;
        }

        public void CommitCollectedItems()
        {
            if (_isCountSavePending == false)
            {
                return;
            }

            _core.SaveChanges();
            _isCountSavePending = false;
        }

        public void ResetCountDirtyFlag()
        {
            _isCountSavePending = false;
        }
    }
}
