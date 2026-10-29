using System;
using MadSlime.Player.Data;
using MadSlime.Player.Interfaces;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Player.Logic
{
    public class Clamper : IRadiusRecipient
    {
        private float _radius;

        private Bounds _bounds;
        private bool _isBoundsAssigned;

        public Clamper(PlayerConfig config)
        {
            ArgumentGuard.CheckNotNull(config, nameof(config));
            SetRadius(config.MovementClampRadius);
        }

        public float Radius => _radius;

        public void SetBounds(Bounds bounds)
        {
            if (bounds.size.x <= 0f || bounds.size.z <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bounds),
                    "Clamper.SetBounds requires positive XZ size.");
            }

            _bounds = bounds;
            _isBoundsAssigned = true;
        }

        public void SetRadius(float radius)
        {
            ArgumentGuard.CheckNonNegative(radius, nameof(radius));

            _radius = radius;
        }

        public Vector3 ClampPosition(Vector3 desiredPosition)
        {
            if (_isBoundsAssigned == false)
            {
                throw new InvalidOperationException(
                    "Clamper: ClampPosition is called before SetBounds. Level preparation must pass the floor bounds.");
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
