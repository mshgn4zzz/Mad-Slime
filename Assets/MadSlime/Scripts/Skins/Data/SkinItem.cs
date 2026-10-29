using UnityEngine;

namespace MadSlime.Skins.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Shop Item", fileName = "NewShopItem")]
    public class SkinItem : ScriptableObject
    {
        [SerializeField] private GameObject _model;

        [SerializeField] private Sprite _icon;

        [SerializeField] private SkinRarity _rarity;

        [SerializeField] private string _id;

        public GameObject Model => _model;

        public Sprite Icon => _icon;

        public SkinRarity Rarity => _rarity;

        public string Id => _id;
    }
}
