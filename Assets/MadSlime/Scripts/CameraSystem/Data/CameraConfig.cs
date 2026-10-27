using Items;
using UnityEngine;

namespace CameraSystem
{
    [CreateAssetMenu(menuName = "Mad Slime/Camera Config", fileName = "NewCameraConfig")]
    public class CameraConfig : ScriptableObject
    {
        [Header("Follow")]
        [SerializeField, Min(0.01f)] private float _catchUpTime = 0.3f;

        [SerializeField, Min(0.01f)] private float _maxSpeed = 150f;

        [SerializeField, Min(0.01f)] private float _minDistance = 1f;

        [Header("Lens")]
        [SerializeField, Range(1f, 179f)] private float _baseFov = 60f;

        [Header("Pull")]
        [SerializeField]
        private AnimationCurve _pullByMass = AnimationCurve.Linear(0f, 0.4f, 50f, 2.5f);

        [SerializeField]
        private float _maxPull = 6f;

        [SerializeField]
        private float _tierPush = 4f;

        [SerializeField]
        private float _maxPush = 8f;

        [SerializeField]
        private float _returnSpeed = 4f;

        [Header("Shake")]
        [SerializeField, Min(0.1f)] private float _shakeSpeed = 40f;

        [SerializeField] private Vector2 _shakeAxes = Vector2.one;

        [SerializeField, Min(0f)]
        private float _shakeFromMass = 1000f;

        [SerializeField, Min(0f)]
        private float _shakeStep = 0.15f;

        [SerializeField, Range(0f, 2f)]
        private float _maxShake = 0.35f;

        [SerializeField, Min(0.1f)]
        private float _shakeFadeSpeed = 7f;

        [Header("FOV Kick")]
        [SerializeField, Min(0f)]
        private float _tierFovKick = 7f;

        [SerializeField, Min(0.1f)]
        private float _fovReturnSpeed = 5f;

        public float PositionSmoothTime => _catchUpTime;

        public float MaxPositionSpeed => _maxSpeed;

        public float MinDistance => _minDistance;

        public float BaseFov => _baseFov;

        public AnimationCurve MassToPullStrength => _pullByMass;

        public float MaxPull => _maxPull;

        public float TierPushStrength => _tierPush;

        public float MaxPush => _maxPush;

        public float RecoverSpeed => _returnSpeed;

        public float ShakeFrequency => _shakeSpeed;

        public Vector2 ShakeAxes => _shakeAxes;

        public float ShakeMassThreshold => _shakeFromMass;

        public float ShakeStrength => _shakeStep;

        public float MaxShake => _maxShake;

        public float ShakeRecoverSpeed => _shakeFadeSpeed;

        public float TierFovKick => _tierFovKick;

        public float FovRecoverSpeed => _fovReturnSpeed;

        private void OnValidate()
        {
            _shakeAxes = new Vector2(Mathf.Clamp(_shakeAxes.x, 0f, 1f), Mathf.Clamp(_shakeAxes.y, 0f, 1f));
        }
    }
}
