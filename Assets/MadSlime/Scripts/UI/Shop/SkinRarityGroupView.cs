using System;
using Game;
using Skins;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop
{
    public class SkinRarityGroupView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Image _background;
        [SerializeField] private Transform _itemsParent;
        [SerializeField, Range(0f, 1f)] private float _backgroundAlpha = 0.18f;

        private SkinRarity _rarity;

        public Transform ItemsParent => _itemsParent;

        private void Awake()
        {
            if (_title == null || _background == null || _itemsParent == null)
            {
                throw new InvalidOperationException($"{name}: assign the rarity group title, background and items parent.");
            }
        }

        private void OnEnable()
        {
            Localization.LanguageChanged += OnLanguageChanged;
            OnLanguageChanged();
        }

        private void OnDisable()
        {
            Localization.LanguageChanged -= OnLanguageChanged;
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
                    _title.text = Localization.Get("skins_common");
                    break;
                case SkinRarity.Rare:
                    _title.text = Localization.Get("skins_rare");
                    break;
                case SkinRarity.Epic:
                    _title.text = Localization.Get("skins_epic");
                    break;
                case SkinRarity.Legendary:
                    _title.text = Localization.Get("skins_legendary");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_rarity), _rarity, "Unknown skin rarity.");
            }
        }
    }
}
