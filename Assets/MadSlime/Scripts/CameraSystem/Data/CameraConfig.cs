using DG.Tweening;
using MadSlime.Items.Data;
using UnityEngine;

namespace MadSlime.CameraSystem.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Camera Config", fileName = "NewCameraConfig")]
    public class CameraConfig : ScriptableObject
    {
        [Header("Follow")]
        [SerializeField, Min(0.01f)] private float _catchUpTime = 0.3f;

        [SerializeField, Min(0.01f)] private float _maxSpeed = 150f;

        [Header("Lens")]
        [SerializeField, Range(1f, 179f)] private float _baseFov = 60f;

        [Header("Shake")]
        [SerializeField, Min(0.01f)] private float _shakeDuration = 0.2f;

        [SerializeField, Min(1)] private int _shakeVibrato = 8;

        [SerializeField] private ShakeRandomnessMode _shakeMode = ShakeRandomnessMode.Full;

        [SerializeField] private Vector2 _shakeAxes = Vector2.one;

        [SerializeField]
        private SizeTier _shakeFromTier = SizeTier.Large;

        [SerializeField, Min(0f)]
        private float _shakeStrength = 0.15f;

        [Header("FOV Kick")]
        [SerializeField, Min(0f)]
        private float _tierFovKick = 7f;

        [SerializeField, Min(0.1f)]
        private float _fovReturnSpeed = 5f;

        public float PositionSmoothTime => _catchUpTime;

        public float MaxPositionSpeed => _maxSpeed;

        public float BaseFov => _baseFov;

        public float ShakeDuration => _shakeDuration;

        public int ShakeVibrato => _shakeVibrato;

        public ShakeRandomnessMode ShakeMode => _shakeMode;

        public Vector2 ShakeAxes => _shakeAxes;

        public SizeTier ShakeMinimumTier => _shakeFromTier;

        public float ShakeStrength => _shakeStrength;

        public float TierFovKick => _tierFovKick;

        public float FovRecoverSpeed => _fovReturnSpeed;
    }
}
