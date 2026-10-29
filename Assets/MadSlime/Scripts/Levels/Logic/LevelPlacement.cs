using MadSlime.Items.Data;
using MadSlime.Items.View;
using UnityEngine;

namespace MadSlime.Levels.Logic
{
    public struct LevelPlacement
    {
        private Item _prefab;
        private ItemDefinition _definition;
        private Vector3 _position;
        private float _scale;

        public LevelPlacement(Item prefab, ItemDefinition definition, Vector3 position, float scale)
        {
            _prefab = prefab;
            _definition = definition;
            _position = position;
            _scale = scale;
        }

        public Item Prefab => _prefab;

        public ItemDefinition Definition => _definition;

        public Vector3 Position => _position;

        public float Scale => _scale;
    }
}
