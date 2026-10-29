using System;
using MadSlime.Items.Data;
using UnityEngine;

namespace MadSlime.Levels.Data
{
    [Serializable]
    public class SpawnZone
    {
        [SerializeField] private SpawnShape _shape = SpawnShape.Circle;

        [SerializeField] private Vector2 _center = Vector2.zero;

        [SerializeField] private float _radius = 3f;

        [SerializeField] private int _count = 8;

        [UnityEngine.Serialization.FormerlySerializedAs("_autoSpacing")]
        [SerializeField] private bool _isAutoSpacing = true;

        [SerializeField] private float _spacing = 0f;

        [SerializeField] private SizeTier _minTier = SizeTier.Small;

        [SerializeField] private SizeTier _maxTier = SizeTier.Small;

        [UnityEngine.Serialization.FormerlySerializedAs("_singleType")]
        [SerializeField] private bool _isSingleType = false;

        public SpawnShape Shape => _shape;

        public Vector2 Center => _center;

        public float Radius => _radius;

        public int Count => _count;

        public bool IsAutoSpacing => _isAutoSpacing;

        public float Spacing => _spacing;

        public SizeTier MinTier => _minTier;

        public SizeTier MaxTier => _maxTier;

        public bool IsSingleType => _isSingleType;
    }
}
