using System;
using UnityEngine;
namespace Collectables
{
    public abstract class OverlapRadiusDetector : MonoBehaviour, Player.IRadiusRecipient
    {
        [SerializeField, Min(0.001f)] protected float _radius = 1.5f;

        public float Radius => _radius;

        public void SetRadius(float radius)
        {
            if (radius <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            _radius = radius;
        }
    }
}
