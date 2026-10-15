using UnityEngine;

namespace Skins
{
    [CreateAssetMenu(menuName = "Mad Slime/Shop Item", fileName = "NewShopItem")]
    public class SkinItem : ScriptableObject
    {
        [Tooltip("Модель скина, инстансится на игрока при выборе.")]
        [SerializeField] private GameObject _model;

        [Tooltip("Иконка скина в магазине.")]
        [SerializeField] private Sprite _icon;

        [Tooltip("Редкость скина. Цвет задаётся в SkinRarityTable, общий шанс этой редкости — в RouletteConfig. Legendary доступен только в ежедневной рулетке и должен входить в Exclusive Skins.")]
        [SerializeField] private SkinRarity _rarity;

        [Tooltip("Уникальный id скина: ключ сейвов. Менять после релиза нельзя.")]
        [SerializeField] private string _id;

        public GameObject Model => _model;

        public Sprite Icon => _icon;

        public SkinRarity Rarity => _rarity;

        public string Id => _id;

    }
}
