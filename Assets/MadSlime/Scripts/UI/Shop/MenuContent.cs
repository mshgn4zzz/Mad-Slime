using Economy;
using Roulette;
using Saves;
using Shop;
using Skins;
using System;
using TMPro;
using UI.HUD;
using UI.Upgrades;
using UnityEngine;
using UnityEngine.UI;
using Upgrades;
using VContainer;
namespace UI.Shop
{
    using Localization = global::Localization.Localization;

    public class MenuContent : MonoBehaviour
    {
        [SerializeField] private SkinsPagePresenter _skinsPage;
        [SerializeField] private UpgradesPagePresenter _upgradesPage;
        [SerializeField] private ShopBalanceView _skinsBalance;
        [SerializeField] private ShopBalanceView _upgradesBalance;
        [SerializeField] private RoulettePreparation _roulettePreparation;
        [SerializeField] private ModelPlacer _preview;
        [SerializeField] private Button _openSkinButton;
        [SerializeField] private TMP_Text _openSkinLabel;
        [SerializeField] private TMP_Text _openSkinPrice;
        [SerializeField] private GameObject _openSkinCost;
        [SerializeField] private TMP_Text _skinsTitle;
        [SerializeField] private TMP_Text _upgradesTitle;
        [SerializeField] private LevelLabelUI _levelLabel;

        private SkinInventory _inventory;
        private ShopContent _content;
        private PlayerUpgrades _upgrades;
        private Wallet _wallet;
        private RouletteConfig _config;
        private ILevelStorage _levelStorage;
        private SavesTransaction _transaction;
        private bool _isPrepared;

        [Inject]
        public void Construct(SkinInventory inventory, ShopContent content, PlayerUpgrades upgrades,
            Wallet wallet, RouletteConfig config, ILevelStorage levelStorage, SavesTransaction transaction)
        {
            _inventory = inventory;
            _content = content;
            _upgrades = upgrades;
            _wallet = wallet;
            _config = config;
            _levelStorage = levelStorage;
            _transaction = transaction;
        }

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
            Localization.LanguageChanged += OnLanguageChanged;
            _wallet.BalanceChanged += OnBalanceChanged;
        }

        private void OnDisable()
        {
            Localization.LanguageChanged -= OnLanguageChanged;
            _wallet.BalanceChanged -= OnBalanceChanged;
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

        private void OnLanguageChanged()
        {
            if (_isPrepared == false)
            {
                return;
            }

            _skinsTitle.text = Localization.Get("tab_skins");
            _upgradesTitle.text = Localization.Get("tab_upgrades");
            RefreshSkinPurchase();
        }

        private void OnBalanceChanged(int previousBalance, int balance)
        {
            if (_isPrepared == true)
            {
                RefreshSkinPurchase();
            }
        }

        private void RefreshSkinPurchase()
        {
            RouletteModel roulette = _roulettePreparation.SkinModel;

            if (roulette == null)
            {
                return;
            }

            bool hasSkins = roulette.HasAvailableSkin();
            _openSkinButton.gameObject.SetActive(hasSkins);

            if (hasSkins == false)
            {
                return;
            }

            _openSkinLabel.text = Localization.Get("skin_open_new");
            _openSkinCost.SetActive(true);
            _openSkinPrice.text = roulette.GetPrice().ToString();
            _openSkinButton.interactable = roulette.CanSpin(false);
        }
    }
}
