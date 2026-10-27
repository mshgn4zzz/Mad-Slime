using Skins;
using System.Collections.Generic;
using UnityEngine;
namespace Shop
{
    [CreateAssetMenu(menuName = "Mad Slime/Shop Content", fileName = "NewShopContent")]
    public class ShopContent : ScriptableObject
    {
        [SerializeField] private List<SkinItem> _skinItems;

        public IReadOnlyList<SkinItem> SkinItems => _skinItems;

    }
}
