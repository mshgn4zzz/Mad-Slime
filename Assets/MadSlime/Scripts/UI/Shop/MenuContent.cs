using System;
using MadSlime.Economy.Logic;
using MadSlime.Roulette.Data;
using MadSlime.Roulette.Logic;
using MadSlime.Saves.Interfaces;
using MadSlime.Shop.Data;
using MadSlime.Shop.View;
using MadSlime.Skins.Data;
using MadSlime.UI.HUD;
using MadSlime.UI.Upgrades;
using MadSlime.Upgrades.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MadSlime.UI.Shop
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    public class MenuContent : MonoBehaviour
    {
        [VContainer.Inject] private Localization _localization;

        [SerializeField] private SkinsPagePresenter _skinsPage;
        [SerializeField] private UpgradesPagePresenter _upgradesPage;
        [SerializeField] private ShopBalanceView _skinsBalance;
        [SerializeField] private ShopBalanceView _upgradesBalance;
        [SerializeField] private ModelPlacer _preview;
        [SerializeField] private Button _openSkinButton;
        [SerializeField] private TMP_Text _openSkinLabel;
        [SerializeField] private TMP_Text _openSkinPrice;
        [SerializeField] private GameObject _openSkinCost;
        [SerializeField] private TMP_Text _skinsTitle;
        [SerializeField] private TMP_Text _upgradesTitle;
        [SerializeField] private LevelLabelUI _levelLabel;
        private RoulettePreparation _roulettePreparation;

        private SkinInventory _inventory;
        private ShopContent _content;
        private PlayerUpgrades _upgrades;
        private Wallet _wallet;
        private RouletteConfig _config;
        private ILevelStorage _levelStorage;
        private SavesTransaction _transaction;
        private bool _isPrepared;

        private void Awake()
        {
            if (_skinsPage == null || _upgradesPage == null || _skinsBalance == null || _upgradesBalance == null
                || _roulettePreparation == null || _preview == null || _openSkinButton == null || _openSkinLabel == null
                || _openSkinPrice == null || _openSkinCost == null || _skinsTitle == null || _upgradesTitle == null
                || _levelLabel == null)
            {
                throw new InvalidOperationException($"{name}: assign all menu content references.");
            }
        }

        private void OnEnable()
        {
            _localization.LanguageChanged += OnLanguageChanged;
            _wallet.BalanceChanged += OnBalanceChanged;
        }

        private void OnDisable()
        {
            _localization.LanguageChanged -= OnLanguageChanged;
            _wallet.BalanceChanged -= OnBalanceChanged;
        }

        [Inject]
        public void Construct(
            SkinInventory inventory,
            ShopContent content,
            PlayerUpgrades upgrades,
            Wallet wallet,
            RouletteConfig config,
            ILevelStorage levelStorage,
            SavesTransaction transaction,
            RoulettePreparation roulettePreparation)
        {
            _roulettePreparation = roulettePreparation;
            _inventory = inventory;
            _content = content;
            _upgrades = upgrades;
            _wallet = wallet;
            _config = config;
            _levelStorage = levelStorage;
            _transaction = transaction;
        }

        public void Prepare()
        {
            _skinsPage.Setup(_inventory, _content.SkinItems, _config.ExclusiveSkins, _config.RarityTable, _preview);
            _upgradesPage.Setup(_upgrades, _wallet, _transaction);
            _skinsBalance.Setup(_wallet);
            _upgradesBalance.Setup(_wallet);
            _levelLabel.SetLevel(_levelStorage.CurrentLevel);
            _isPrepared = true;
            OnLanguageChanged();
        }

        public void RefreshSkins(string rewardSkinId)
        {
            if (_roulettePreparation.SkinModel != null)
            {
                _roulettePreparation.SkinModel.RefreshSlots();
            }

            _skinsPage.Refresh(rewardSkinId);
            RefreshSkinPurchase();
        }

        private void RefreshSkinPurchase()
        {
            RouletteModel roulette = _roulettePreparation.SkinModel;

            if (roulette == null)
            {
                return;
            }

            bool isSkinAvailable = roulette.IsSkinAvailable();

            _openSkinButton.gameObject.SetActive(isSkinAvailable);

            if (isSkinAvailable == false)
            {
                return;
            }

            _openSkinLabel.text = _localization.Get("skin_open_new");
            _openSkinCost.SetActive(true);
            _openSkinPrice.text = roulette.GetPrice().ToString();
            _openSkinButton.interactable = roulette.IsSpinAvailable(false);
        }

        private void OnLanguageChanged()
        {
            if (_isPrepared == false)
            {
                return;
            }

            _skinsTitle.text = _localization.Get("tab_skins");
            _upgradesTitle.text = _localization.Get("tab_upgrades");
            RefreshSkinPurchase();
        }

        private void OnBalanceChanged(int previousBalance, int balance)
        {
            if (_isPrepared)
            {
                RefreshSkinPurchase();
            }
        }
    }
}
