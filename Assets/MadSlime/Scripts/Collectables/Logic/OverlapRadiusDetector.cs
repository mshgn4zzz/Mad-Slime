using MadSlime.Player.Interfaces;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Collectables.Logic
{
    public abstract class OverlapRadiusDetector : MonoBehaviour, IRadiusRecipient
    {
        [SerializeField, Min(0.001f)] private float _detectionRadius = 1.5f;

        public float Radius => _detectionRadius;

        public void SetRadius(float radius)
        {
            ArgumentGuard.CheckPositive(radius, nameof(radius));

            _detectionRadius = radius;
        }
    }
}
