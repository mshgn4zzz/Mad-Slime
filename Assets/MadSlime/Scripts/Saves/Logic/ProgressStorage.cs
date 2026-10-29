using MadSlime.Saves.Interfaces;
using MadSlime.Validation;
using UnityEngine;
using YG;

namespace MadSlime.Saves.Logic
{
    public class ProgressStorage : ILevelStorage, IPlayerProgressStorage
    {
        private SavesCore _core;
        private CollectedItemsStorage _collectedItems;

        public ProgressStorage(SavesCore core, CollectedItemsStorage collectedItems)
        {
            ArgumentGuard.CheckNotNull(core, nameof(core));
            ArgumentGuard.CheckNotNull(collectedItems, nameof(collectedItems));

            _core = core;
            _collectedItems = collectedItems;
        }

        public int CurrentLevel => _core.LoadData().CurrentLevel;

        public int MaxLevel => _core.LoadData().MaxLevel;

        public void SetLevelProgress(int currentLevel, int maxLevel)
        {
            ArgumentGuard.CheckPositive(currentLevel, nameof(currentLevel));

            ArgumentGuard.CheckRange(maxLevel, currentLevel, int.MaxValue, nameof(maxLevel));

            SavesYG data = _core.LoadData();

            data.CurrentLevel = currentLevel;
            data.MaxLevel = maxLevel;
            _core.SaveChanges();
        }

        public void ResetProgress()
        {
            _core.ExecuteTransaction(ApplyProgressReset);
            _collectedItems.ResetCountDirtyFlag();
        }

        private void ApplyProgressReset()
        {
            SavesYG data = _core.LoadData();
            SavesYG initialData = new SavesYG();

            initialData.idSave = data.idSave;
            initialData.Language = data.Language;
            initialData.MusicVolume = data.MusicVolume;
            initialData.SfxVolume = data.SfxVolume;
            initialData.Balance = 0;
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(initialData), data);
        }
    }
}
