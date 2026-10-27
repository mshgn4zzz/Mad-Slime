using System;
using UnityEngine;
using YG;

namespace Saves
{
    public class ProgressStorage : ILevelStorage, IPlayerProgressStorage
    {
        private SavesCore _core;
        private CollectedItemsStorage _collectedItems;

        public ProgressStorage(SavesCore core, CollectedItemsStorage collectedItems)
        {
            if (core == null || collectedItems == null)
            {
                throw new ArgumentNullException(nameof(core),
                    "ProgressStorage requires a SavesCore and a CollectedItemsStorage instance.");
            }

            _core = core;
            _collectedItems = collectedItems;
        }

        public int CurrentLevel => _core.LoadData().CurrentLevel;

        public int MaxLevel => _core.LoadData().MaxLevel;

        public void SetLevelProgress(int currentLevel, int maxLevel)
        {
            if (currentLevel <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentLevel),
                    currentLevel,
                    "ProgressStorage.SetLevelProgress requires a positive current level.");
            }

            if (maxLevel < currentLevel)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLevel),
                    maxLevel,
                    "ProgressStorage.SetLevelProgress requires maxLevel to be greater or equal to currentLevel.");
            }

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
            initialData.musicVolume = data.musicVolume;
            initialData.sfxVolume = data.sfxVolume;
            initialData.Balance = 0;
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(initialData), data);
        }
    }
}
