using Items;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Levels
{
    [CreateAssetMenu(menuName = "Mad Slime/Prop Set", fileName = "NewPropSet")]
    public class PropSet : ScriptableObject
    {
        [Tooltip("Базовые предметы-пропсы уровня.")]
        [SerializeField] private List<Item> _props = new List<Item>();

        [Tooltip("Варианты пропсов: тот же префаб с другим определением (иконка и тир).")]
        [SerializeField] private List<PropVariant> _variants = new List<PropVariant>();

        public IReadOnlyList<Item> Props => _props;
        public IReadOnlyList<PropVariant> Variants => _variants;
    }

}
