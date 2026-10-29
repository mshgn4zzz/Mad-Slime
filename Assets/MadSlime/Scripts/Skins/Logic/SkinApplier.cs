using System;
using MadSlime.Saves.Interfaces;
using MadSlime.Shop.Data;
using MadSlime.Skins.Data;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Skins.Logic
{
    public class SkinApplier : MonoBehaviour
    {
        [SerializeField] private ShopContent _shopContent;
        [SerializeField] private Transform _skinsContainer;

        private ISkinStorage _skinStorage;
        private SkinInventory _inventory;
        private ISavesReadiness _savesReadiness;
        private SkinModelFactory _modelFactory = new SkinModelFactory();
        private GameObject _currentModel;

        public event Action<GameObject> ModelChanged;

        public GameObject CurrentModel => _currentModel;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_shopContent, name, nameof(_shopContent));

            ArgumentGuard.CheckAssigned(_skinsContainer, name, nameof(_skinsContainer));

            ArgumentGuard.CheckAssigned(_skinStorage, name, nameof(_skinStorage));
            ArgumentGuard.CheckAssigned(_inventory, name, nameof(_inventory));
            ArgumentGuard.CheckAssigned(_savesReadiness, name, nameof(_savesReadiness));
        }

        private void OnEnable()
        {
            _savesReadiness.Prepared += OnSavesLoaded;
        }

        private void Start()
        {
            ApplySelectedSkin();
        }

        private void OnDisable()
        {
            _savesReadiness.Prepared -= OnSavesLoaded;
        }

        private void OnDestroy()
        {
            if (_currentModel != null)
            {
                Destroy(_currentModel);
            }
        }

        [Inject]
        public void Construct(ISkinStorage skinStorage, ISavesReadiness savesReadiness, SkinInventory inventory)
        {
            _skinStorage = skinStorage;
            _inventory = inventory;
            _savesReadiness = savesReadiness;
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

                ArgumentGuard.CheckAssigned(matchingItem, name, nameof(matchingItem));
                ArgumentGuard.CheckAssigned(matchingItem.Model, name, nameof(matchingItem.Model));

                _inventory.TrySelect(matchingItem.Id);
            }

            if (_currentModel != null)
            {
                Destroy(_currentModel);
            }

            _currentModel = _modelFactory.Create(matchingItem.Model, _skinsContainer);
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

        private void OnSavesLoaded()
        {
            ApplySelectedSkin();
        }
    }
}
