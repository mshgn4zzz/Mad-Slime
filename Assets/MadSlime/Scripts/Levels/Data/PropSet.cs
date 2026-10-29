using System.Collections.Generic;
using MadSlime.Items.View;
using UnityEngine;

namespace MadSlime.Levels.Data
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
