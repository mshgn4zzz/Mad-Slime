using MadSlime.Validation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MadSlime.UI.Upgrades
{
    public class UpgradeItemViewFactory
    {
        private readonly UpgradeItemView _upgradeItemViewPrefab;

        private readonly IObjectResolver _resolver;

        public UpgradeItemViewFactory(IObjectResolver resolver, UpgradeItemView upgradeItemViewPrefab)
        {
            ArgumentGuard.CheckNotNull(resolver, nameof(resolver));
            ArgumentGuard.CheckNotNull(upgradeItemViewPrefab, nameof(upgradeItemViewPrefab));

            _resolver = resolver;
            _upgradeItemViewPrefab = upgradeItemViewPrefab;
        }

        public UpgradeItemView Create(Transform parent)
        {
            return _resolver.Instantiate(_upgradeItemViewPrefab, parent);
        }
    }
}
