using UnityEngine;

namespace MadSlime.ShapeFill.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Fill Config", fileName = "NewFillConfig")]
    public class FillConfig : ScriptableObject
    {
        [Header("Fill")]
        [SerializeField, Min(0f)] private float _fillDelay = 0.55f;

        [SerializeField, Min(0.001f)] private float _spawnInterval = 0.04f;

        [SerializeField, Min(0.1f)] private float _maxFillDuration = 8f;

        [SerializeField, Min(0.01f)] private float _flightDuration = 0.5f;

        [Header("Tap Boost")]
        [SerializeField, Min(0.001f)] private float _boostedSpawnInterval = 0.015f;

        [SerializeField, Min(0.1f)] private float _maxBoostedTime = 3f;

        [SerializeField, Min(0.01f)] private float _boostedFlightDuration = 0.25f;

        [Header("Tap Instant")]
        [SerializeField, Min(0.05f)] private float _instantFillDuration = 0.25f;

        [SerializeField, Min(0.01f)] private float _instantFlightDuration = 0.1f;

        [Header("Bonus Wave")]
        [SerializeField, Min(0f)] private float _bonusWaveDelay = 0.35f;

        [SerializeField] private Color _bonusColor = Color.black;

        [SerializeField, Range(0f, 1f)] private float _bonusColorMix = 0.3f;

        [Header("Border")]
        [SerializeField, Min(0.05f)] private float _borderSpawnTime = 0.5f;

        [SerializeField] private Color _borderColor = Color.black;

        [Header("Ghost")]
        [SerializeField, Range(0f, 1f)] private float _ghostAlpha = 0.4f;

        [Header("Shape Punch")]
        [SerializeField, Range(0f, 0.3f)] private float _shapePunch = 0.06f;

        [SerializeField, Min(0.01f)] private float _shapePunchDuration = 0.4f;

        [SerializeField, Min(0)] private int _shapePunchShakes = 10;

        [SerializeField, Range(0f, 1f)] private float _shapePunchSpring = 0.3f;

        [Header("Camera Kick")]
        [SerializeField, Min(0f)] private float _fovKick = 9f;

        [SerializeField, Min(0.01f)] private float _fovDuration = 0.6f;

        public float FillDelay => _fillDelay;

        public float SpawnInterval => _spawnInterval;

        public float MaxFillDuration => _maxFillDuration;

        public float FlightDuration => _flightDuration;

        public float BoostedSpawnInterval => _boostedSpawnInterval;

        public float MaxBoostedFillDuration => _maxBoostedTime;

        public float BoostedFlightDuration => _boostedFlightDuration;

        public float InstantFillDuration => _instantFillDuration;

        public float InstantFlightDuration => _instantFlightDuration;

        public float BonusWaveDelay => _bonusWaveDelay;

        public Color BonusTintColor => _bonusColor;

        public float BonusTintStrength => _bonusColorMix;

        public float BorderCascadeDuration => _borderSpawnTime;

        public Color BorderColor => _borderColor;

        public float GhostOpacity => _ghostAlpha;

        public float ShapePunchStrength => _shapePunch;

        public float ShapePunchDuration => _shapePunchDuration;

        public int ShapePunchVibrato => _shapePunchShakes;

        public float ShapePunchElasticity => _shapePunchSpring;

        public float FovKick => _fovKick;

        public float FovDuration => _fovDuration;
    }
}
