using System;
using MadSlime.Skins.Data;
using MadSlime.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Shop
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    public class SkinRarityGroupView : MonoBehaviour
    {
        [VContainer.Inject] private Localization _localization;

        [SerializeField] private TMP_Text _title;
        [SerializeField] private Image _background;
        [SerializeField] private Transform _itemsParent;
        [SerializeField, Range(0f, 1f)] private float _backgroundAlpha = 0.18f;

        private SkinRarity _rarity;

        public Transform ItemsParent => _itemsParent;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_title, name, nameof(_title));
            ArgumentGuard.CheckAssigned(_background, name, nameof(_background));
            ArgumentGuard.CheckAssigned(_itemsParent, name, nameof(_itemsParent));
        }

        private void OnEnable()
        {
            _localization.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void OnDisable()
        {
            _localization.LanguageChanged -= OnLanguageChanged;
        }

        public void Initialize(SkinRarity rarity, Color color)
        {
            _rarity = rarity;
            _background.color = new Color(color.r, color.g, color.b, _backgroundAlpha);
            OnLanguageChanged();
        }

        private void OnLanguageChanged()
        {
            switch (_rarity)
            {
                case SkinRarity.Common:
                    _title.text = _localization.Get("skins_common");
                    break;

                case SkinRarity.Rare:
                    _title.text = _localization.Get("skins_rare");
                    break;

                case SkinRarity.Epic:
                    _title.text = _localization.Get("skins_epic");
                    break;

                case SkinRarity.Legendary:
                    _title.text = _localization.Get("skins_legendary");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_rarity), _rarity, "Unknown skin rarity.");
            }
        }
    }
}
