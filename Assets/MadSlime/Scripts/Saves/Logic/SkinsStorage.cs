using System;
using System.Collections.Generic;
using MadSlime.Saves.Interfaces;
using MadSlime.Validation;

namespace MadSlime.Saves.Logic
{
    public class SkinsStorage : ISkinStorage
    {
        private SavesCore _core;

        public SkinsStorage(SavesCore core)
        {
            ArgumentGuard.CheckNotNull(core, nameof(core));

            _core = core;
        }

        public string SelectedSkinId => _core.LoadData().SelectedSkinId;

        public void SelectSkin(string skinId)
        {
            ArgumentGuard.CheckNotEmpty(skinId, nameof(skinId));

            _core.LoadData().SelectedSkinId = skinId;
            _core.SaveChanges();
        }

        public bool IsSkinOpen(string skinId)
        {
            return _core.LoadData().OpenSkinIds.Contains(skinId);
        }

        public void OpenSkin(string skinId)
        {
            List<string> openSkinIds = _core.LoadData().OpenSkinIds;

            if (openSkinIds.Contains(skinId))
            {
                throw new InvalidOperationException(
                    $"SkinsStorage.OpenSkin: skin '{skinId}' is already open.");
            }

            openSkinIds.Add(skinId);
            _core.SaveChanges();
        }

        public List<string> ReadOpenSkinIds()
        {
            return new List<string>(_core.LoadData().OpenSkinIds);
        }

        public List<string> ReadShowcaseSkinIds()
        {
            return new List<string>(_core.LoadData().ShowcaseSkinIds);
        }

        public void SetShowcaseSkinIds(IReadOnlyList<string> showcaseSkinIds)
        {
            ArgumentGuard.CheckNotNull(showcaseSkinIds, nameof(showcaseSkinIds));

            List<string> savedShowcase = _core.LoadData().ShowcaseSkinIds;

            savedShowcase.Clear();

            for (int i = 0; i < showcaseSkinIds.Count; i++)
            {
                savedShowcase.Add(showcaseSkinIds[i]);
            }

            _core.SaveChanges();
        }
    }
}
