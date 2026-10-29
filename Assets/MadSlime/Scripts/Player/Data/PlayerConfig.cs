using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace MadSlime.Player.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Player Config", fileName = "NewPlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement Bounds")]
        [SerializeField, Min(0f)] private float _movementClampRadius = 0.5f;

        [Header("Absorption")]
        [SerializeField, Min(0.05f)] private float _minAbsorbDuration = 0.1f;

        [SerializeField, Min(0.05f)] private float _maxAbsorbDuration = 0.45f;

        [SerializeField, Min(0.5f)] private float _absorbSpeed = 9f;

        [SerializeField, Range(0f, 0.5f)] private float _arcLow = 0.1f;

        [SerializeField, Range(0f, 0.5f)] private float _arcHigh = 0.3f;

        [SerializeField, Min(0f)] private float _minSpinSpeed = 120f;

        [SerializeField, Min(0f)] private float _maxSpinSpeed = 540f;

        [SerializeField, Min(0f)] private float _smokeSpeed;

        [SerializeField, Range(0.1f, 0.95f)] private float _shrinkStart = 0.6f;

        [SerializeField] private Ease _absorbEase = Ease.InQuad;

        [Header("Collect Squash")]
        [SerializeField, Min(0.01f)] private float _squashDuration = 0.2f;

        [SerializeField]
        private AnimationCurve _squashCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.35f, 0.88f),
            new Keyframe(1f, 1f));

        [Header("Collect Sound")]
        [SerializeField, Min(0f)] private float _pickupSoundMinInterval = 0.05f;

        [SerializeField, Min(0f)] private float _pickupSoundMaxInterval = 0.1f;

        [SerializeField, Range(0.1f, 3f)] private float _pickupSoundMinPitch = 0.95f;

        [SerializeField, Range(0.1f, 3f)] private float _pickupSoundMaxPitch = 1.15f;

        [Header("Movement Squash")]
        [SerializeField, Range(0f, 0.5f)] private float _crawlSquash = 0.22f;

        [Header("Movement Cycle")]
        [SerializeField]
        private AnimationCurve _crawlCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.35f, 1f),
            new Keyframe(0.7f, 0.4f),
            new Keyframe(1f, 0f));

        [SerializeField, Min(0.01f)] private float _crawlRate = 1.6f;

        [SerializeField, Min(1f)] private float _finalCrawlTimes = 2f;

        [SerializeField, Min(0.01f)] private float _crawlSoftness = 0.15f;

        [Header("Growth")]
        [SerializeField, Min(0)] private int _startMass;

        [SerializeField] private List<PlayerTierThreshold> _tiers = new List<PlayerTierThreshold>();

        public float MovementClampRadius => _movementClampRadius;

        public float MinAbsorbDuration => _minAbsorbDuration;

        public float MaxAbsorbDuration => _maxAbsorbDuration;

        public float AbsorbSpeed => _absorbSpeed;

        public float MinArcFraction => _arcLow;

        public float MaxArcFraction => _arcHigh;

        public float MinSpinSpeed => _minSpinSpeed;

        public float MaxSpinSpeed => _maxSpinSpeed;

        public float SmokeSpeedThreshold => _smokeSpeed;

        public float ShrinkStart => _shrinkStart;

        public Ease AbsorbEase => _absorbEase;

        public float SquashDuration => _squashDuration;

        public AnimationCurve SquashCurve => _squashCurve;

        public float PickupSoundMinInterval => _pickupSoundMinInterval;

        public float PickupSoundMaxInterval => _pickupSoundMaxInterval;

        public float PickupSoundMinPitch => _pickupSoundMinPitch;

        public float PickupSoundMaxPitch => _pickupSoundMaxPitch;

        public float DeformMaxStretch => _crawlSquash;

        public AnimationCurve CrawlStretchCurve => _crawlCurve;

        public float MovementSquashFrequency => _crawlRate;

        public float FinalSquashFrequencyMultiplier => _finalCrawlTimes;

        public float CrawlRampTime => _crawlSoftness;

        public int StartMass => _startMass;

        public IReadOnlyList<PlayerTierThreshold> Thresholds => _tiers;
    }
}
