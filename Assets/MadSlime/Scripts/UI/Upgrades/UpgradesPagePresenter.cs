using System;
using System.Collections.Generic;
using System.Globalization;
using MadSlime.Economy.Logic;
using MadSlime.Saves.Interfaces;
using MadSlime.Upgrades.Data;
using MadSlime.Upgrades.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.UI.Upgrades
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    public class UpgradesPagePresenter : MonoBehaviour
    {
        private const float MillionsPriceThreshold = 1000000f;
        private const float ThousandsPriceThreshold = 1000f;
        [VContainer.Inject] private Localization _localization;

        [SerializeField] private Transform _upgradesParent;
        [SerializeField] private Transform _perksParent;
        private UpgradeItemViewFactory _upgradeFactory;

        private PlayerUpgrades _upgrades;
        private Wallet _wallet;
        private UpgradePurchaseModel _purchases;
        private List<UpgradeItemView> _upgradeViews = new List<UpgradeItemView>();
        private List<UpgradeItemView> _perkViews = new List<UpgradeItemView>();

        private void OnEnable()
        {
            if (_purchases == null)
            {
                return;
            }

            Show();

            foreach (UpgradeItemView view in _upgradeViews)
            {
                view.Clicked -= OnClicked;
                view.Clicked += OnClicked;
            }

            foreach (UpgradeItemView view in _perkViews)
            {
                view.Clicked -= OnClicked;
                view.Clicked += OnClicked;
            }

            _wallet.BalanceChanged -= OnBalanceChanged;
            _wallet.BalanceChanged += OnBalanceChanged;
        }

        private void OnDisable()
        {
            if (_wallet != null)
            {
                _wallet.BalanceChanged -= OnBalanceChanged;
            }

            foreach (UpgradeItemView view in _upgradeViews)
            {
                view.Clicked -= OnClicked;
            }

            foreach (UpgradeItemView view in _perkViews)
            {
                view.Clicked -= OnClicked;
            }

            Clear();
        }

        [Inject]
        public void Construct(UpgradeItemViewFactory factory)
        {
            _upgradeFactory = factory;
        }

        public void Setup(PlayerUpgrades upgrades, Wallet wallet, SavesTransaction transaction)
        {
            ArgumentGuard.CheckAssigned(_upgradesParent, name, nameof(_upgradesParent));
            ArgumentGuard.CheckAssigned(_perksParent, name, nameof(_perksParent));
            ArgumentGuard.CheckAssigned(_upgradeFactory, name, nameof(_upgradeFactory));

            OnDisable();
            _upgrades = upgrades;
            _wallet = wallet;
            _purchases = new UpgradePurchaseModel(upgrades, wallet, transaction);

            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void Show()
        {
            if (_upgradeViews.Count > 0 || _perkViews.Count > 0)
            {
                return;
            }

            foreach (UpgradeEntry entry in _upgrades.Config.Upgrades)
            {
                UpgradeItemView view = _upgradeFactory.Create(_upgradesParent);

                view.Initialize(entry.Icon, _localization.Get(GetUpgradeKey(entry.Type)), string.Empty, true);
                _upgradeViews.Add(view);
                UpdateUpgrade(view, entry.Type);
            }

            foreach (PerkEntry entry in _upgrades.Config.Perks)
            {
                UpgradeItemView view = _upgradeFactory.Create(_perksParent);

                view.Initialize(
                    entry.Icon,
                    _localization.Get(GetPerkKey(entry.Type)),
                    _localization.Get(GetPerkDescKey(entry.Type)),
                    false);
                _perkViews.Add(view);
                UpdatePerk(view, entry.Type);
            }
        }

        private void UpdateUpgrade(UpgradeItemView view, UpgradeType type)
        {
            float value = _upgrades.GetTotalValue(type);

            if (_upgrades.IsMaxed(type) == false)
            {
                value = _upgrades.GetNextStepValue(type);
            }

            view.SetProgress(
                string.Format(
                _localization.Get("upgrade_step"),
                _upgrades.GetLevel(type),
                _upgrades.GetMaxSteps(type)),
                string.Format(_localization.Get(GetUpgradeDescKey(type)), FormatPercent(value)));
            SetUpgradeState(view, type);
        }

        private void SetUpgradeState(UpgradeItemView view, UpgradeType type)
        {
            if (_upgrades.IsMaxed(type))
            {
                view.SetPurchaseState(_localization.Get("shop_max"), false);
                return;
            }

            view.SetPurchaseState(FormatPrice(_upgrades.GetNextCost(type)), _purchases.IsPurchaseAvailable(type));
        }

        private void UpdatePerk(UpgradeItemView view, PerkType type)
        {
            if (_upgrades.IsPerkPurchased(type))
            {
                view.SetPurchaseState(_localization.Get("shop_purchased"), false);
                return;
            }

            view.SetPurchaseState(FormatPrice(_upgrades.GetPerkCost(type)), _purchases.IsPurchaseAvailable(type));
        }

        private void Clear()
        {
            foreach (UpgradeItemView view in _upgradeViews)
            {
                Destroy(view.gameObject);
            }

            foreach (UpgradeItemView view in _perkViews)
            {
                Destroy(view.gameObject);
            }

            _upgradeViews.Clear();
            _perkViews.Clear();
        }

        private string FormatPercent(float value)
        {
            int percent = Mathf.RoundToInt(value * 100f);

            return percent.ToString(CultureInfo.InvariantCulture);
        }

        private string FormatPrice(int amount)
        {
            if (amount >= MillionsPriceThreshold)
            {
                return FormatFraction(amount / MillionsPriceThreshold) + "m";
            }

            if (amount >= ThousandsPriceThreshold)
            {
                return FormatFraction(amount / ThousandsPriceThreshold) + "k";
            }

            return amount.ToString(CultureInfo.InvariantCulture);
        }

        private string FormatFraction(float value)
        {
            return value.ToString("0.#", CultureInfo.InvariantCulture);
        }

        private string GetUpgradeKey(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Speed:
                    return "upgrade_speed";

                case UpgradeType.Appetite:
                    return "upgrade_appetite";

                case UpgradeType.Taste:
                    return "upgrade_taste";

                case UpgradeType.Metabolism:
                    return "upgrade_metabolism";

                default:
                    throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "UpgradesPagePresenter received an unknown upgrade type.");
            }
        }

        private string GetUpgradeDescKey(UpgradeType type)
        {
            return GetUpgradeKey(type) + "_desc";
        }

        private string GetPerkKey(PerkType type)
        {
            switch (type)
            {
                case PerkType.Smell:
                    return "perk_smell";

                case PerkType.Adrenaline:
                    return "perk_adrenaline";

                case PerkType.Ambitions:
                    return "perk_ambitions";

                default:
                    throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "UpgradesPagePresenter received an unknown perk type.");
            }
        }

        private string GetPerkDescKey(PerkType type)
        {
            return GetPerkKey(type) + "_desc";
        }

        private void OnClicked(UpgradeItemView view)
        {
            int index = _upgradeViews.IndexOf(view);

            if (index >= 0)
            {
                UpgradeType type = _upgrades.Config.Upgrades[index].Type;

                if (_purchases.TryPurchase(type))
                {
                    UpdateUpgrade(view, type);
                }

                return;
            }

            index = _perkViews.IndexOf(view);

            if (index >= 0)
            {
                PerkType type = _upgrades.Config.Perks[index].Type;

                if (_purchases.TryPurchase(type))
                {
                    UpdatePerk(view, type);
                }
            }
        }

        private void OnBalanceChanged(int previousBalance, int currentBalance)
        {
            for (int i = 0; i < _upgradeViews.Count; i++)
            {
                SetUpgradeState(_upgradeViews[i], _upgrades.Config.Upgrades[i].Type);
            }

            for (int i = 0; i < _perkViews.Count; i++)
            {
                UpdatePerk(_perkViews[i], _upgrades.Config.Perks[i].Type);
            }
        }
    }
}
