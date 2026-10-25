using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Shop
{
    public class ShopItemViewFactory : MonoBehaviour
    {
        [SerializeField] private ShopItemView _shopItemViewPrefab;
        [SerializeField] private SkinRarityGroupView _rarityGroupPrefab;

        private IObjectResolver _resolver;

        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }

        private void Awake()
        {
            if (_resolver == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Resolver was not injected. MenuLifetimeScope must be the first object in the scene hierarchy.");
            }
        }

        public SkinRarityGroupView GetRarityGroup(Transform parent)
        {
            return _resolver.Instantiate(_rarityGroupPrefab, parent);
        }

        public ShopItemView Get(Transform parent)
        {
            return _resolver.Instantiate(_shopItemViewPrefab, parent);
        }
    }
}
