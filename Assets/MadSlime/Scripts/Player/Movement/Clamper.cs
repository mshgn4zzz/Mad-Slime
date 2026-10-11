using System;
using Player;
using UnityEngine;

namespace Movement
{
    public class Clamper : MonoBehaviour, IRadiusRecipient
    {
        [Tooltip("Радиус тела слайма для отступа от границ уровня; скейлер масштабирует его вместе с тиром.")]
        [SerializeField, Min(0f)] private float _radius = 0.5f;

        private Bounds _bounds;
        private bool _hasBounds;

        public float Radius => _radius;

        public void SetBounds(Bounds bounds)
        {
            if (bounds.size.x <= 0f || bounds.size.z <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(bounds),
                    "Clamper.SetBounds requires positive XZ size.");
            }

            _bounds = bounds;
            _hasBounds = true;
        }

        public void SetRadius(float radius)
        {
            if (radius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radius),
                    "Clamper.SetRadius requires a non-negative radius.");
            }

            _radius = radius;
        }

        public Vector3 ClampPosition(Vector3 desiredPosition)
        {
            if (_hasBounds == false)
            {
                throw new InvalidOperationException(
                    $"{name}: ClampPosition is called before SetBounds. Level preparation must pass the floor bounds.");
            }

            Vector3 clamped = desiredPosition;

            float allowedRadiusX = Mathf.Min(_radius, _bounds.extents.x);
            float allowedRadiusZ = Mathf.Min(_radius, _bounds.extents.z);
            clamped.x = Mathf.Clamp(clamped.x, _bounds.min.x + allowedRadiusX, _bounds.max.x - allowedRadiusX);
            clamped.z = Mathf.Clamp(clamped.z, _bounds.min.z + allowedRadiusZ, _bounds.max.z - allowedRadiusZ);

            return clamped;
        }
    }
}
