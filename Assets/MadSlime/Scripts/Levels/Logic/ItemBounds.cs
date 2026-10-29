using System;
using MadSlime.Items.View;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Levels.Logic
{
    public static class ItemBounds
    {
        public static float GetRadiusXZ(Item prefab)
        {
            ArgumentGuard.CheckNotNull(prefab, nameof(prefab));

            if (prefab.TryGetComponent(out ItemBody body) == false
                || (body.Collider is BoxCollider boxCollider) == false)
            {
                throw new InvalidOperationException(
                    $"{prefab.name}: Item prefab needs a BoxCollider in the Collider field on "
                    + $"ItemBody to measure the XZ radius.");
            }

            Vector3 scaledSize = Vector3.Scale(boxCollider.size, prefab.transform.lossyScale);

            Vector3 scaledCenter = Vector3.Scale(boxCollider.center, prefab.transform.lossyScale);
            float footprintRadius = new Vector2(scaledSize.x, scaledSize.z).magnitude * 0.5f;
            float pivotOffset = new Vector2(scaledCenter.x, scaledCenter.z).magnitude;

            return footprintRadius + pivotOffset;
        }
    }
}
