using System;
using Saves;

namespace Skins
{
    public class SkinInventory
    {
        public const string InitialSkinId = "Slime";

        private ISkinStorage _storage;

        public string SelectedSkinId => _storage.SelectedSkinId;

        public SkinInventory(ISkinStorage storage)
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            _storage = storage;
        }

        public void Initialize()
        {
            if (_storage.ReadOpenSkinIds().Contains(InitialSkinId) == false)
            {
                _storage.OpenSkin(InitialSkinId);
            }

            if (string.IsNullOrWhiteSpace(_storage.SelectedSkinId) == true || IsOpen(_storage.SelectedSkinId) == false)
            {
                _storage.SelectSkin(InitialSkinId);
            }
        }

        public bool IsOpen(string skinId)
        {
            return skinId == InitialSkinId || _storage.IsSkinOpen(skinId);
        }

        public bool Grant(string skinId)
        {
            if (IsOpen(skinId) == true)
            {
                return false;
            }

            _storage.OpenSkin(skinId);
            return true;
        }

        public bool Select(string skinId)
        {
            if (IsOpen(skinId) == false)
            {
                return false;
            }

            if (SelectedSkinId != skinId)
            {
                _storage.SelectSkin(skinId);
            }

            return true;
        }
    }
}
