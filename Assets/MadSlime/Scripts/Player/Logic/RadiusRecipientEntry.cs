using System;
using UnityEngine;
namespace Player
{
    [Serializable]
    public class RadiusRecipientEntry
    {
        [Tooltip("Получатель радиуса: детектор, клампер и т.п. — реализует IRadiusRecipient. "
            + "Базовый радиус берётся из компонента получателя на старте и умножается на масштаб тира.")]
        [SerializeField] private MonoBehaviour _recipient;

        private IRadiusRecipient _radiusRecipient;
        private float _baseRadius;

        public IRadiusRecipient Recipient => _radiusRecipient;
        public float BaseRadius => _baseRadius;

        public void Initialize(string ownerName)
        {
            if (_recipient == null)
            {
                throw new InvalidOperationException(
                    $"{ownerName}: a radius recipient entry has no recipient assigned.");
            }

            if (_recipient is IRadiusRecipient radiusRecipient == false)
            {
                throw new InvalidOperationException(
                    $"{ownerName}: recipient '{_recipient.name}' does not implement IRadiusRecipient.");
            }

            _radiusRecipient = radiusRecipient;
            _baseRadius = _radiusRecipient.Radius;
        }
    }
}
