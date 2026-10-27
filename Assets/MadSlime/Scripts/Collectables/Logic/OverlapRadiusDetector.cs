using System;
using UnityEngine;

namespace Collectables
{
    public abstract class OverlapRadiusDetector : MonoBehaviour, Player.IRadiusRecipient
    {
        [SerializeField, Min(0.001f)] protected float DetectionRadius = 1.5f;

        public float Radius => DetectionRadius;

        public void SetRadius(float radius)
        {
            if (radius <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            DetectionRadius = radius;
        }
    }
}
