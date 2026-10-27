using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Mad Slime/Item Definition", fileName = "NewItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private Sprite _icon;

        [SerializeField] private SizeTier _tier = SizeTier.Small;

        public Sprite Icon => _icon;

        public SizeTier Tier => _tier;
    }
}
