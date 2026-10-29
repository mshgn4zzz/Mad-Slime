using MadSlime.Quota.Logic;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.ShapeFill.Logic
{
    public class FillResultCalculator
    {
        private readonly QuotaCounter _quotaCounter;

        public FillResultCalculator(QuotaCounter quotaCounter)
        {
            _quotaCounter = quotaCounter;

            ArgumentGuard.CheckNotNull(_quotaCounter, nameof(quotaCounter));
        }

        public FillResult Calculate(int requiredCubes, Vector3 formCenter, Vector3 formSize)
        {
            int quotaCubes = CalculateQuotaFill(requiredCubes);
            int bonusCubes = CalculateBonusFill(requiredCubes, quotaCubes);

            return new FillResult(quotaCubes, bonusCubes, requiredCubes, formCenter, formSize);
        }

        private int CalculateQuotaFill(int maxCubes)
        {
            if (_quotaCounter.TotalQuotaTarget <= 0)
            {
                return 0;
            }

            float quotaPercent = Mathf.Clamp01(
                _quotaCounter.CollectedQuotaCount / (float)_quotaCounter.TotalQuotaTarget);

            return Mathf.Clamp(Mathf.RoundToInt(quotaPercent * maxCubes), 0, maxCubes);
        }

        private int CalculateBonusFill(int maxCubes, int quotaCubes)
        {
            int totalCount = Mathf.Clamp(Mathf.RoundToInt(_quotaCounter.GetFillPercent() * maxCubes), 0, maxCubes);

            return Mathf.Max(0, totalCount - quotaCubes);
        }
    }
}
