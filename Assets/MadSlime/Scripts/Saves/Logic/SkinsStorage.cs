using System;
using System.Collections.Generic;

namespace Saves
{
    public class SkinsStorage : ISkinStorage
    {
        private SavesCore _core;

        public SkinsStorage(SavesCore core)
        {
            if (core == null)
            {
                throw new ArgumentNullException(nameof(core),
                    "SkinsStorage requires a SavesCore instance.");
            }

            _core = core;
        }

        public string SelectedSkinId => _core.LoadData().SelectedSkinId;

        public void SelectSkin(string skinId)
        {
            if (string.IsNullOrEmpty(skinId) == true)
            {
                throw new ArgumentException(
                    "SkinsStorage.SelectSkin requires a non-empty skin id.",
                    nameof(skinId));
            }

            _core.LoadData().SelectedSkinId = skinId;
            _core.SaveChanges();
        }

        public bool IsSkinOpen(string skinId)
        {
            return _core.LoadData()._openSkinIds.Contains(skinId);
        }

        public void OpenSkin(string skinId)
        {
            List<string> openSkinIds = _core.LoadData()._openSkinIds;

            if (openSkinIds.Contains(skinId) == true)
            {
                throw new InvalidOperationException(
                    $"SkinsStorage.OpenSkin: skin '{skinId}' is already open.");
            }

            openSkinIds.Add(skinId);
            _core.SaveChanges();
        }

        public List<string> ReadOpenSkinIds()
        {
            return new List<string>(_core.LoadData()._openSkinIds);
        }

        public List<string> ReadShowcaseSkinIds()
        {
            return new List<string>(_core.LoadData()._showcaseSkinIds);
        }

        public void SetShowcaseSkinIds(IReadOnlyList<string> showcaseSkinIds)
        {
            if (showcaseSkinIds == null)
            {
                throw new ArgumentNullException(nameof(showcaseSkinIds),
                    "SkinsStorage.SetShowcaseSkinIds requires a showcase id list.");
            }

            List<string> savedShowcase = _core.LoadData()._showcaseSkinIds;

            savedShowcase.Clear();

            for (int i = 0; i < showcaseSkinIds.Count; i++)
            {
                savedShowcase.Add(showcaseSkinIds[i]);
            }

            _core.SaveChanges();
        }
    }
}
