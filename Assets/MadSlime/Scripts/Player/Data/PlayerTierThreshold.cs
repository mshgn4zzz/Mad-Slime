using System;
using MadSlime.Items.Data;
using UnityEngine;

namespace MadSlime.Player.Data
{
    [Serializable]
    public class PlayerTierThreshold
    {
        [SerializeField] private SizeTier _tier;

        [SerializeField] private int _mass;

        [SerializeField] private float _growTimes = 1f;

        [SerializeField] private float _speed = 1f;

        [SerializeField] private float _cameraZoom = 1f;

        public SizeTier Tier => _tier;

        public int RequiredMass => _mass;

        public float ScaleMultiplier => _growTimes;

        public float CameraOffsetMultiplier => _cameraZoom;

        public float Speed => _speed;
    }
}
