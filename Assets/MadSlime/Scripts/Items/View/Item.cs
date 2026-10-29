using System;
using MadSlime.Items.Data;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Items.View
{
    [RequireComponent(typeof(ItemBody))]
    public class Item : MonoBehaviour
    {
        [SerializeField] private ItemDefinition _definition;

        private bool _isInitialized;

        public ItemDefinition Definition => _definition;

        public void Initialize(ItemDefinition definition)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("Item is already initialized.");
            }

            ArgumentGuard.CheckNotNull(definition, nameof(definition));

            _definition = definition;
            _isInitialized = true;
        }
    }
}
