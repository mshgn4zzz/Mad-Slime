using MadSlime.Validation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MadSlime.UI.Shop
{
    public class ShopItemViewFactory
    {
        private readonly ShopItemView _shopItemViewPrefab;
        private readonly SkinRarityGroupView _rarityGroupPrefab;

        private readonly IObjectResolver _resolver;

        public ShopItemViewFactory(
            IObjectResolver resolver,
            ShopItemView shopItemViewPrefab,
            SkinRarityGroupView rarityGroupPrefab)
        {
            ArgumentGuard.CheckNotNull(resolver, nameof(resolver));
            ArgumentGuard.CheckNotNull(shopItemViewPrefab, nameof(shopItemViewPrefab));
            ArgumentGuard.CheckNotNull(rarityGroupPrefab, nameof(rarityGroupPrefab));

            _resolver = resolver;
            _shopItemViewPrefab = shopItemViewPrefab;
            _rarityGroupPrefab = rarityGroupPrefab;
        }

        public SkinRarityGroupView CreateRarityGroup(Transform parent)
        {
            return _resolver.Instantiate(_rarityGroupPrefab, parent);
        }

        public ShopItemView Create(Transform parent)
        {
            return _resolver.Instantiate(_shopItemViewPrefab, parent);
        }
    }
}
