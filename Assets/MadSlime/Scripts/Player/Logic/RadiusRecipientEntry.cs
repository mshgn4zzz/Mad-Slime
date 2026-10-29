using System;
using MadSlime.Player.Interfaces;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Player.Logic
{
    [Serializable]
    public class RadiusRecipientEntry
    {
        [SerializeField] private MonoBehaviour _recipient;

        private IRadiusRecipient _radiusRecipient;
        private float _baseRadius;

        public IRadiusRecipient Recipient => _radiusRecipient;

        public float BaseRadius => _baseRadius;

        public void Initialize(string ownerName)
        {
            ArgumentGuard.CheckAssigned(_recipient, ownerName, nameof(_recipient));

            if ((_recipient is IRadiusRecipient radiusRecipient) == false)
            {
                throw new InvalidOperationException(
                    $"{ownerName}: recipient '{_recipient.name}' does not implement IRadiusRecipient.");
            }

            _radiusRecipient = radiusRecipient;
            _baseRadius = _radiusRecipient.Radius;
        }
    }
}
