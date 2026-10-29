using System.Collections.Generic;
using MadSlime.Shop.View;
using MadSlime.Skins.Data;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MadSlime.UI.Shop
{
    public class SkinsPagePresenter : MonoBehaviour
    {
        private const int ScrollLayoutFrameCount = 3;

        [SerializeField] private Transform _itemsParent;
        [SerializeField] private ScrollRect _scroll;
        private ShopItemViewFactory _factory;

        private SkinInventory _inventory;
        private SkinRarityLookup _rarityLookup;
        private ModelPlacer _preview;
        private List<SkinItem> _skins = new List<SkinItem>();
        private List<ShopItemView> _views = new List<ShopItemView>();
        private List<SkinRarityGroupView> _groups = new List<SkinRarityGroupView>();
        private bool _isSetup;
        private int _scrollLayoutFrames;
        private Vector2 _pendingScrollPosition;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_itemsParent, name, nameof(_itemsParent));
            ArgumentGuard.CheckAssigned(_scroll, name, nameof(_scroll));
        }

        private void OnEnable()
        {
            if (_isSetup == false)
            {
                return;
            }

            Show();

            foreach (ShopItemView view in _views)
            {
                view.Clicked -= OnClicked;
                view.Clicked += OnClicked;
            }
        }

        private void OnDisable()
        {
            foreach (ShopItemView view in _views)
            {
                view.Clicked -= OnClicked;
            }

            Clear();

            if (_preview != null)
            {
                _preview.Clear();
            }
        }

        private void LateUpdate()
        {
            if (_scrollLayoutFrames <= 0)
            {
                return;
            }

            _scrollLayoutFrames--;

            if (_scrollLayoutFrames == 0)
            {
                Canvas.ForceUpdateCanvases();
                _scroll.normalizedPosition = _pendingScrollPosition;
            }
        }

        [Inject]
        public void Construct(ShopItemViewFactory factory)
        {
            ArgumentGuard.CheckNotNull(factory, nameof(factory));

            _factory = factory;
        }

        public void Setup(
            SkinInventory inventory,
            IEnumerable<SkinItem> skins,
            IEnumerable<SkinItem> exclusiveSkins,
            SkinRarityTable rarityTable,
            ModelPlacer preview)
        {
            ArgumentGuard.CheckNotNull(inventory, nameof(inventory));
            ArgumentGuard.CheckNotNull(skins, nameof(skins));
            ArgumentGuard.CheckNotNull(exclusiveSkins, nameof(exclusiveSkins));
            ArgumentGuard.CheckNotNull(rarityTable, nameof(rarityTable));
            ArgumentGuard.CheckNotNull(preview, nameof(preview));

            OnDisable();
            _inventory = inventory;
            _rarityLookup = new SkinRarityLookup(rarityTable);
            _preview = preview;
            _skins.Clear();
            AddSkins(skins);
            AddSkins(exclusiveSkins);
            _skins.Sort(CompareRarity);
            _isSetup = true;

            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        public void Refresh(string rewardSkinId)
        {
            Vector2 position;

            if (rewardSkinId == null)
            {
                position = new Vector2(0f, 1f);
            }
            else
            {
                position = _scroll.normalizedPosition;
            }

            OnDisable();
            OnEnable();

            foreach (ShopItemView view in _views)
            {
                view.SetHighlighted(view.SkinId == rewardSkinId);
            }

            Canvas.ForceUpdateCanvases();
            _scroll.normalizedPosition = position;
            _pendingScrollPosition = position;
            _scrollLayoutFrames = ScrollLayoutFrameCount;
        }

        private int CompareRarity(SkinItem left, SkinItem right)
        {
            return left.Rarity.CompareTo(right.Rarity);
        }

        private void AddSkins(IEnumerable<SkinItem> skins)
        {
            foreach (SkinItem skin in skins)
            {
                if (skin == null || FindSkin(skin.Id) != null)
                {
                    continue;
                }

                _skins.Add(skin);
            }
        }

        private void Show()
        {
            if (_views.Count > 0)
            {
                return;
            }

            int index = 0;

            while (index < _skins.Count)
            {
                int end = index;
                SkinRarity rarity = _skins[index].Rarity;

                while (end < _skins.Count && _skins[end].Rarity == rarity)
                {
                    end++;
                }

                SkinRarityGroupView group = _factory.CreateRarityGroup(_itemsParent);

                group.Initialize(rarity, _rarityLookup.Get(rarity).PlateColor);
                _groups.Add(group);
                SpawnGroup(group.ItemsParent, index, end, true);
                SpawnGroup(group.ItemsParent, index, end, false);
                index = end;
            }

            SkinItem equipped = FindSkin(_inventory.SelectedSkinId);

            if (equipped != null)
            {
                _preview.SetModel(equipped.Model);
            }

            Canvas.ForceUpdateCanvases();
            _scroll.verticalNormalizedPosition = 1f;
            _pendingScrollPosition = new Vector2(0f, 1f);
            _scrollLayoutFrames = ScrollLayoutFrameCount;
        }

        private void SpawnGroup(Transform parent, int start, int end, bool isOpen)
        {
            for (int i = start; i < end; i++)
            {
                SkinItem skin = _skins[i];

                if (_inventory.IsOpen(skin.Id) != isOpen)
                {
                    continue;
                }

                ShopItemView view = _factory.Create(parent);

                view.Initialize(
                    skin.Id,
                    skin.Icon,
                    _rarityLookup.Get(skin.Rarity).PlateColor,
                    isOpen,
                    _inventory.SelectedSkinId == skin.Id);
                _views.Add(view);
            }
        }

        private SkinItem FindSkin(string skinId)
        {
            foreach (SkinItem skin in _skins)
            {
                if (skin.Id == skinId)
                {
                    return skin;
                }
            }

            return null;
        }

        private void Clear()
        {
            foreach (ShopItemView view in _views)
            {
                view.gameObject.SetActive(false);
            }

            _views.Clear();

            foreach (SkinRarityGroupView group in _groups)
            {
                group.gameObject.SetActive(false);
                Destroy(group.gameObject);
            }

            _groups.Clear();
        }

        private void OnClicked(ShopItemView view)
        {
            SkinItem skin = FindSkin(view.SkinId);

            _preview.SetModel(skin.Model);

            if (_inventory.TrySelect(skin.Id) == false)
            {
                return;
            }

            foreach (ShopItemView item in _views)
            {
                item.SetSelected(item.SkinId == _inventory.SelectedSkinId);
            }
        }
    }
}
