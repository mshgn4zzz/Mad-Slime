using System.Collections.Generic;
using MadSlime.Skins.Data;
using UnityEngine;

namespace MadSlime.Shop.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Shop Content", fileName = "NewShopContent")]
    public class ShopContent : ScriptableObject
    {
        [SerializeField] private List<SkinItem> _skinItems;

        public IReadOnlyList<SkinItem> SkinItems => _skinItems;
    }
}
