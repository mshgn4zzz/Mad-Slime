using System;
using Saves;
using Shop;
using UnityEngine;
using VContainer;

namespace Skins
{
    public class SkinApplier : MonoBehaviour
    {
        [SerializeField] private ShopContent _shopContent;
        [SerializeField] private Transform _skinsContainer;

        public event Action<GameObject> ModelChanged;

        public GameObject CurrentModel => _currentModel;

        private ISkinStorage _skinStorage;
        private SkinInventory _inventory;
        private ISavesReadiness _savesReadiness;
        private GameObject _currentModel;

        [Inject]
        public void Construct(ISkinStorage skinStorage, ISavesReadiness savesReadiness, SkinInventory inventory)
        {
            _skinStorage = skinStorage;
            _inventory = inventory;
            _savesReadiness = savesReadiness;
        }

        private void Awake()
        {
            if (_shopContent == null)
            {
                throw new InvalidOperationException(
                    $"{name}: SkinApplier requires _shopContent to be assigned in the inspector.");
            }

            if (_skinsContainer == null)
            {
                throw new InvalidOperationException(
                    $"{name}: SkinApplier requires _skinsContainer to be assigned in the inspector.");
            }

            if (_skinStorage == null || _inventory == null || _savesReadiness == null)
            {
                throw new InvalidOperationException(
                    $"{name}: skin dependencies were not injected. Check that ProjectLifetimeScope "
                        + $"registers SkinsStorage/ISavesReadiness and SkinInventory, and that "
                        + $"SkinApplier is registered in a LifetimeScope.");
            }
        }

        private void OnEnable()
        {
            _savesReadiness.Ready += OnSavesLoaded;
        }

        private void Start()
        {
            ApplySelectedSkin();
        }

        private void OnDisable()
        {
            _savesReadiness.Ready -= OnSavesLoaded;
        }

        private void OnDestroy()
        {
            if (_currentModel != null)
            {
                Destroy(_currentModel);
            }
        }

        private void OnSavesLoaded()
        {
            ApplySelectedSkin();
        }

        private void ApplySelectedSkin()
        {
            if (_savesReadiness.IsReady == false)
            {
                return;
            }

            _inventory.Initialize();

            string selectedId = _skinStorage.SelectedSkinId;

            SkinItem matchingItem = FindItem(selectedId);

            if (matchingItem == null || matchingItem.Model == null)
            {
                matchingItem = FindItem(SkinInventory.InitialSkinId);

                if (matchingItem == null || matchingItem.Model == null)
                {
                    throw new InvalidOperationException(
                        $"{name}: the initial skin '{SkinInventory.InitialSkinId}' and its "
                            + $"model must exist in ShopContent.");
                }

                _inventory.Select(matchingItem.Id);
            }

            if (_currentModel != null)
            {
                Destroy(_currentModel);
            }

            _currentModel = Instantiate(matchingItem.Model, _skinsContainer);
            ModelChanged?.Invoke(_currentModel);
        }

        private SkinItem FindItem(string skinId)
        {
            foreach (SkinItem item in _shopContent.SkinItems)
            {
                if (item == null)
                {
                    continue;
                }

                if (item.Id == skinId)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
