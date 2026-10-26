using Items;
using System;
using UnityEngine;
namespace Levels
{
    [Serializable]
    public class PropVariant
    {
        [Tooltip("Префаб предмета.")]
        [SerializeField] private Item _prefab;

        [Tooltip("Определение, присваиваемое варианту: иконка и тир.")]
        [SerializeField] private ItemDefinition _definition;

        public PropVariant(Item prefab, ItemDefinition definition)
        {
            _prefab = prefab;
            _definition = definition;
        }

        public Item Prefab => _prefab;
        public ItemDefinition Definition => _definition;
    }
}
