using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Levels
{
    [CreateAssetMenu(menuName = "Mad Slime/Prop Set", fileName = "NewPropSet")]
    public class PropSet : ScriptableObject
    {
        [SerializeField] private List<Item> _props = new List<Item>();

        [SerializeField] private List<PropVariant> _variants = new List<PropVariant>();

        public IReadOnlyList<Item> Props => _props;

        public IReadOnlyList<PropVariant> Variants => _variants;
    }
}
