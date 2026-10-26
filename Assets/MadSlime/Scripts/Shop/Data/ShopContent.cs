using Skins;
using System.Collections.Generic;
using UnityEngine;
namespace Shop
{
    [CreateAssetMenu(menuName = "Mad Slime/Shop Content", fileName = "NewShopContent")]
    public class ShopContent : ScriptableObject
    {
        [Tooltip("Список скинов, доступных в магазине. Дубли и пустые id запрещены.")]
        [SerializeField] private List<SkinItem> _skinItems;

        public IReadOnlyList<SkinItem> SkinItems => _skinItems;

    }
}
