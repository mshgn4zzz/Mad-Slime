using Items;
using System.Collections.Generic;
using UnityEngine;
namespace Levels
{
    [CreateAssetMenu(menuName = "Mad Slime/Layout Set", fileName = "NewLayoutSet")]
    public class LayoutSet : ScriptableObject
    {
        [SerializeField] private List<SpawnZone> _zones = new List<SpawnZone>();

        [SerializeField] private bool _allowMirroring = true;

        [Header("Item Placement")]
        [SerializeField] private float _autoGapTimes = 2.2f;

        [SerializeField] private float _scatterGapTimes = 0.7f;

        [SerializeField, Min(0f)] private float _startClearRadius = 10f;

        [SerializeField, Min(0f)] private float _smallItemStartClearRadius = 2f;

        [SerializeField, Min(0f)] private float _largeItemGap = 1f;

        public float StartClearRadius => _startClearRadius;
        public float SmallItemStartClearRadius => _smallItemStartClearRadius;
        public float LargeItemGap => _largeItemGap;
        public IReadOnlyList<SpawnZone> Zones => _zones;
        public bool AllowMirroring => _allowMirroring;
        public float AutoSpacingFactor => _autoGapTimes;
        public float ScatterDistanceFactor => _scatterGapTimes;
    }
}
