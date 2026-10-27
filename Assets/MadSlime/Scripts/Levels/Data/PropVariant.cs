using System;
using Items;
using UnityEngine;

namespace Levels
{
    [Serializable]
    public class PropVariant
    {
        [SerializeField] private Item _prefab;

        [SerializeField] private ItemDefinition _item;

        public PropVariant(Item prefab, ItemDefinition definition)
        {
            _prefab = prefab;
            _item = definition;
        }

        public Item Prefab => _prefab;

        public ItemDefinition Definition => _item;
    }
}
