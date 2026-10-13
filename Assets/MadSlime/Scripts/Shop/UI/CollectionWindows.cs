using System;
using Audio;
using Game;
using Roulette;
using Saves;
using Scriptables;
using Skins;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Upgrades;
using VContainer;

namespace Shop
{
    public class CollectionWindows : MonoBehaviour
    {
        private enum WindowState
        {
            Closed,
            Skins,
            Upgrades,
            SkinRoulette
        }

        [SerializeField] private GameObject _windowRoot;
        [SerializeField] private SkinsPagePresenter _skinsPage;
        [SerializeField] private UpgradesPagePresenter _upgradesPage;
        [SerializeField] private ShopBalanceView _balance;
        [SerializeField] private RoulettePresenter _roulette;
        [SerializeField] private ModelPlacer _preview;
        [SerializeField] private Camera _previewCamera;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _openSkinButton;
        [SerializeField] private TMP_Text _title;

        private WindowState _state;
        private SkinInventory _inventory;
        private ShopContent _content;
        private PlayerUpgrades _upgrades;
        private Wallet _wallet;
        private RouletteConfig _config;
        private RouletteFactory _factory;
        private AdScheduler _ads;
        private IUISoundPlayer _sound;
        private UiSpawner _spawner;
        private ISaveConfirmation _saveConfirmation;
        private Action<Action> _transaction;
        private string _rewardSkinId;

        [Inject]
        public void Construct(SkinInventory inventory, ShopContent content, PlayerUpgrades upgrades,
            Wallet wallet, RouletteConfig config, RouletteFactory factory, AdScheduler ads,
            IUISoundPlayer sound, UiSpawner spawner, ISaveConfirmation saveConfirmation, Action<Action> transaction)
        {
            _inventory = inventory;
            _content = content;
            _upgrades = upgrades;
            _wallet = wallet;
            _config = config;
            _factory = factory;
            _ads = ads;
            _sound = sound;
            _spawner = spawner;
            _saveConfirmation = saveConfirmation;
            _transaction = transaction;
        }

        private void Awake()
        {
            if (_windowRoot == null || _skinsPage == null || _upgradesPage == null || _balance == null
                || _roulette == null || _preview == null || _previewCamera == null || _closeButton == null
                || _openSkinButton == null || _title == null)
            {
                throw new InvalidOperationException($"{name}: assign all collection window references.");
            }
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnCloseClicked);
            _openSkinButton.onClick.AddListener(OnOpenSkinClicked);
            _roulette.Closed += OnRouletteClosed;
            _roulette.PrizeClaimed += OnPrizeClaimed;
            Localization.LanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(OnCloseClicked);
            _openSkinButton.onClick.RemoveListener(OnOpenSkinClicked);
            _roulette.Closed -= OnRouletteClosed;
            _roulette.PrizeClaimed -= OnPrizeClaimed;
            Localization.LanguageChanged -= OnLanguageChanged;
        }

        public void Setup()
        {
            int sectorCount = _roulette.View.Wheel.SectorCount;
            _roulette.Setup(_factory.CreateSkins(_content.SkinItems, sectorCount),
                _ads, _sound, _spawner, _saveConfirmation);
            _skinsPage.Setup(_inventory, _content.SkinItems, _config.ExclusiveSkins, _config.RarityTable, _preview);
            _upgradesPage.Setup(_upgrades, _wallet, _transaction);
            _balance.Setup(_wallet);
            SetState(WindowState.Closed);
        }

        public void OpenSkins()
        {
            SetState(WindowState.Skins);
        }

        public void OpenUpgrades()
        {
            SetState(WindowState.Upgrades);
        }

        public void Close()
        {
            if (_windowRoot == null || _previewCamera == null)
            {
                _state = WindowState.Closed;
                return;
            }

            SetState(WindowState.Closed);
            _roulette.Hide();
        }

        public bool HasWindow()
        {
            return _state != WindowState.Closed;
        }

        private void OnOpenSkinClicked()
        {
            if (_state != WindowState.Skins)
            {
                return;
            }

            _rewardSkinId = null;
            SetState(WindowState.SkinRoulette);
            _roulette.Show();
        }

        private void OnCloseClicked()
        {
            if (_state == WindowState.SkinRoulette)
            {
                _roulette.Hide();
                return;
            }

            Close();
        }

        private void OnRouletteClosed()
        {
            if (_state != WindowState.SkinRoulette)
            {
                return;
            }

            SetState(WindowState.Skins);
            _skinsPage.Refresh(_rewardSkinId);
        }

        private void OnPrizeClaimed(RoulettePrize prize)
        {
            if (prize.Skin != null)
            {
                _rewardSkinId = prize.Skin.Id;
            }
        }

        private void SetState(WindowState state)
        {
            _state = state;
            bool isSkins = state == WindowState.Skins || state == WindowState.SkinRoulette;
            bool isRoulette = state == WindowState.SkinRoulette;
            _windowRoot.SetActive(state != WindowState.Closed);
            _skinsPage.gameObject.SetActive(isSkins);
            _upgradesPage.gameObject.SetActive(state == WindowState.Upgrades);
            _roulette.gameObject.SetActive(isRoulette);
            _previewCamera.gameObject.SetActive(state == WindowState.Skins);
            _openSkinButton.gameObject.SetActive(state == WindowState.Skins);

            OnLanguageChanged();

            if (state == WindowState.Closed)
            {
                _preview.Clear();
            }
        }

        private void OnLanguageChanged()
        {
            switch (_state)
            {
                case WindowState.Skins:
                    _title.text = Localization.Get("tab_skins");
                    break;
                case WindowState.Upgrades:
                    _title.text = Localization.Get("tab_upgrades");
                    break;
                case WindowState.SkinRoulette:
                    _title.text = Localization.Get("skin_roulette_title");
                    break;
                case WindowState.Closed:
                    break;
            }
        }
    }
}
