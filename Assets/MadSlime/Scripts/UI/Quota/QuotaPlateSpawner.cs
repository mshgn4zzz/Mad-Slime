using MadSlime.Quota.Data;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.UI.Quota
{
    public class QuotaPlateSpawner
    {
        private QuotaPlateUI _platePrefab;
        private RectTransform _container;

        public QuotaPlateSpawner(QuotaPlateUI platePrefab, RectTransform container)
        {
            ArgumentGuard.CheckNotNull(platePrefab, nameof(platePrefab));

            ArgumentGuard.CheckNotNull(container, nameof(container));

            _platePrefab = platePrefab;
            _container = container;
        }

        public QuotaPlateUI Spawn(QuotaEntry entry)
        {
            QuotaPlateUI plate = UnityEngine.Object.Instantiate(_platePrefab, _container);

            plate.Setup(entry.Definition.Icon, entry.GetRemaining());

            return plate;
        }
    }
}
