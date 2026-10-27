using System;
using Items;
using UnityEngine;

namespace Levels
{
    [Serializable]
    public class SpawnZone
    {
        [SerializeField] private SpawnShape _shape = SpawnShape.Circle;

        [SerializeField] private Vector2 _center = Vector2.zero;

        [SerializeField] private float _radius = 3f;

        [SerializeField] private int _count = 8;

        [SerializeField] private bool _autoSpacing = true;

        [SerializeField] private float _spacing = 0f;

        [SerializeField] private SizeTier _minTier = SizeTier.Small;

        [SerializeField] private SizeTier _maxTier = SizeTier.Small;

        [SerializeField] private bool _singleType = false;

        public SpawnShape Shape => _shape;

        public Vector2 Center => _center;

        public float Radius => _radius;

        public int Count => _count;

        public bool AutoSpacing => _autoSpacing;

        public float Spacing => _spacing;

        public SizeTier MinTier => _minTier;

        public SizeTier MaxTier => _maxTier;

        public bool SingleType => _singleType;
    }
}
