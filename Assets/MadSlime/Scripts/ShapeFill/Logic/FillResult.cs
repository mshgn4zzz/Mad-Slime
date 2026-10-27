using System;
using UnityEngine;

namespace ShapeFill
{
    public class FillResult
    {
        public int QuotaCubes { get; }

        public int BonusCubes { get; }

        public int TargetCubes { get; }

        public float Percent { get; }

        public bool IsWin { get; }

        public Vector3 FormCenter { get; }

        public Vector3 FormSize { get; }

        public FillResult(int quotaCubes, int bonusCubes, int requiredCubes, Vector3 formCenter, Vector3 formSize)
        {
            if (quotaCubes < 0 || bonusCubes < 0 || requiredCubes <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredCubes),
                    "Fill result requires a built nonempty shape and "
                        + "nonnegative counts.");
            }

            if (formSize.x <= 0f || formSize.y <= 0f || formSize.z <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(formSize), "Fill result requires a positive form size.");
            }

            QuotaCubes = quotaCubes;
            BonusCubes = bonusCubes;
            TargetCubes = Mathf.Clamp(quotaCubes + bonusCubes, 0, requiredCubes);
            Percent = Mathf.Clamp01(TargetCubes / (float)requiredCubes);
            IsWin = Percent >= 1f;
            FormCenter = formCenter;
            FormSize = formSize;
        }
    }
}
