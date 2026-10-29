using System;
using MadSlime.Roulette.Logic;
using MadSlime.Skins.Data;
using MadSlime.UI.Animations;
using MadSlime.UI.Windows;
using MadSlime.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Roulette
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    [RequireComponent(typeof(UiScaleAnimator))]
    public class RouletteWinPopup : BaseWindow
    {
        private const float FadeAlpha = 0.935f;
        [VContainer.Inject] private Localization _localization;

        [SerializeField] private RectTransform _panel;
        [SerializeField] private Image _fade;
        [SerializeField]
        private Image _skinIcon;
        [SerializeField]
        private Image _coinIcon;
        [SerializeField]
        private Image _prizeFrame;
        [SerializeField]
        private Sprite _normalFrame;
        [SerializeField]
        private Sprite _rareFrame;
        [SerializeField]
        private Sprite _epicFrame;
        [SerializeField]
        private Sprite _legendaryFrame;
        [SerializeField] private Image _plate;
        [SerializeField]
        private Sprite _normalPlate;
        [SerializeField]
        private Sprite _rarePlate;
        [SerializeField]
        private Sprite _epicPlate;
        [SerializeField]
        private Sprite _legendaryPlate;
        [SerializeField] private TMP_Text _plateText;
        [SerializeField] private Button _takeButton;
        [SerializeField]
        private Color _normalFrameColor = new Color32(113, 198, 221, 255);
        [SerializeField]
        private Color _rareFrameColor = new Color32(167, 228, 90, 255);
        [SerializeField]
        private Color _epicFrameColor = new Color32(197, 165, 232, 255);
        [SerializeField]
        private Color _legendaryFrameColor = new Color32(246, 198, 75, 255);

        protected override void Awake()
        {
            base.Awake();

            if (_panel == null || _fade == null || _skinIcon == null || _coinIcon == null || _prizeFrame == null
                || _plate == null || _plateText == null || _takeButton == null)
            {
                throw new InvalidOperationException(
                    $"{name}: a win popup part is not assigned. Drag the Panel, Fade, SkinIcon, CoinIcon, "
                    + $"PrizeFrame, Plate, PlateText and TakeButton into the fields.");
            }

            if (_normalFrame == null || _rareFrame == null || _epicFrame == null || _legendaryFrame == null)
            {
                throw new InvalidOperationException(
                    $"{name}: a prize frame sprite is not assigned. Drag the four frame sprites into the fields.");
            }

            if (_normalPlate == null || _rarePlate == null || _epicPlate == null || _legendaryPlate == null)
            {
                throw new InvalidOperationException(
                    $"{name}: a plate sprite is not assigned. Drag the GUI Kit grade label sprites "
                    + $"(blue, green, purple, orange) into the fields.");
            }
        }

        protected override void OnDisable()
        {
            _takeButton.onClick.RemoveListener(OnTakeClicked);
            base.OnDisable();
        }

        private void OnEnable()
        {
            _takeButton.onClick.AddListener(OnTakeClicked);
        }

        public override void Show()
        {
            _takeButton.interactable = true;
            _fade.color = new Color(0f, 0f, 0f, FadeAlpha);
            base.Show();
        }

        public void Initialize(RoulettePrize prize, RouletteView source)
        {
            ArgumentGuard.CheckNotNull(prize, nameof(prize));

            ArgumentGuard.CheckNotNull(source, nameof(source));

            if (prize.Skin != null)
            {
                InitializeSkin(prize.Skin, prize.Skin.Rarity, _localization.Get(GetRarityKey(prize.Skin.Rarity)));
            }
            else
            {
                InitializeCoins(prize.Coins, source.GetCoinIcon(prize.Coins));
            }
        }

        private void InitializeSkin(SkinItem skin, SkinRarity rarity, string rarityName)
        {
            ArgumentGuard.CheckNotNull(skin, nameof(skin));

            ArgumentGuard.CheckAssigned(skin.Icon, skin.name, nameof(skin.Icon));

            _skinIcon.sprite = skin.Icon;
            _skinIcon.gameObject.SetActive(true);
            _coinIcon.gameObject.SetActive(false);

            ShowRarity(rarity, rarityName);
        }

        private void InitializeCoins(int amount, Sprite icon)
        {
            ArgumentGuard.CheckNotNull(icon, nameof(icon));

            _coinIcon.sprite = icon;
            _coinIcon.preserveAspect = true;
            _skinIcon.gameObject.SetActive(false);
            _coinIcon.gameObject.SetActive(true);

            ShowRarity(SkinRarity.Common, $"×{amount}");
        }

        private void ShowRarity(SkinRarity rarity, string label)
        {
            _plate.sprite = GetPlateSprite(rarity);
            _prizeFrame.sprite = GetFrameSprite(rarity);
            _prizeFrame.color = GetFrameColor(rarity);
            _plateText.text = label;
        }

        private Color GetFrameColor(SkinRarity rarity)
        {
            switch (rarity)
            {
                case SkinRarity.Rare:
                    return _rareFrameColor;

                case SkinRarity.Epic:
                    return _epicFrameColor;

                case SkinRarity.Legendary:
                    return _legendaryFrameColor;

                default:
                    return _normalFrameColor;
            }
        }

        private Sprite GetPlateSprite(SkinRarity rarity)
        {
            switch (rarity)
            {
                case SkinRarity.Rare:
                    return _rarePlate;

                case SkinRarity.Epic:
                    return _epicPlate;

                case SkinRarity.Legendary:
                    return _legendaryPlate;

                default:
                    return _normalPlate;
            }
        }

        private Sprite GetFrameSprite(SkinRarity rarity)
        {
            switch (rarity)
            {
                case SkinRarity.Rare:
                    return _rareFrame;

                case SkinRarity.Epic:
                    return _epicFrame;

                case SkinRarity.Legendary:
                    return _legendaryFrame;

                default:
                    return _normalFrame;
            }
        }

        private string GetRarityKey(SkinRarity rarity)
        {
            switch (rarity)
            {
                case SkinRarity.Rare:
                    return "rarity_rare";

                case SkinRarity.Epic:
                    return "rarity_epic";

                case SkinRarity.Legendary:
                    return "rarity_legendary";

                default:
                    return "rarity_common";
            }
        }

        private void OnTakeClicked()
        {
            _takeButton.interactable = false;
            RequestClose();
        }
    }
}
