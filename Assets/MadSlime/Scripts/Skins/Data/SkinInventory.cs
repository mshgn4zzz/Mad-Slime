using MadSlime.Saves.Interfaces;
using MadSlime.Validation;

namespace MadSlime.Skins.Data
{
    public class SkinInventory
    {
        public const string InitialSkinId = "Slime";

        private ISkinStorage _storage;

        public SkinInventory(ISkinStorage storage)
        {
            ArgumentGuard.CheckNotNull(storage, nameof(storage));

            _storage = storage;
        }

        public string SelectedSkinId => _storage.SelectedSkinId;

        public void Initialize()
        {
            if (_storage.ReadOpenSkinIds().Contains(InitialSkinId) == false)
            {
                _storage.OpenSkin(InitialSkinId);
            }

            if (string.IsNullOrWhiteSpace(_storage.SelectedSkinId) || IsOpen(_storage.SelectedSkinId) == false)
            {
                _storage.SelectSkin(InitialSkinId);
            }
        }

        public bool IsOpen(string skinId)
        {
            return skinId == InitialSkinId || _storage.IsSkinOpen(skinId);
        }

        public bool TryGrant(string skinId)
        {
            if (IsOpen(skinId))
            {
                return false;
            }

            _storage.OpenSkin(skinId);
            return true;
        }

        public bool TrySelect(string skinId)
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
