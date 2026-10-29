using UnityEngine;

namespace MadSlime.ShapeFill.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Fill Player Config", fileName = "FillPlayerConfig")]
    public class FillPlayerConfig : ScriptableObject
    {
        [Header("Size")]
        [SerializeField, Min(0.1f)] private float _sizeMultiplier = 1f;

        [Header("Screen Placement")]
        [SerializeField, Range(0.05f, 0.3f)] private float _viewportWidth = 0.2f;
        [SerializeField, Range(0.05f, 0.25f)] private float _maxViewportHeight = 0.16f;
        [SerializeField, Range(0.1f, 0.7f)] private float _cameraDepthFraction = 0.45f;

        [Header("Motion")]
        [SerializeField, Min(0f)] private float _minRotationSpeed = 12f;
        [SerializeField, Min(0f)] private float _maxRotationSpeed = 28f;
        [SerializeField, Range(0f, 0.2f)] private float _pulseAmplitude = 0.05f;
        [SerializeField, Min(0.01f)] private float _pulseFrequency = 0.8f;
        [SerializeField, Range(0f, 0.3f)] private float _hoverAmplitude = 0.12f;
        [SerializeField, Min(0.01f)] private float _hoverFrequency = 0.45f;

        [Header("Emission")]
        [SerializeField, Range(0f, 0.3f)] private float _spawnRadiusFraction = 0.08f;
        [SerializeField, Range(1f, 6f)] private float _departureRadiusMultiplier = 4f;

        public float SizeMultiplier => _sizeMultiplier;
        public float ViewportWidth => _viewportWidth;
        public float MaxViewportHeight => _maxViewportHeight;
        public float CameraDepthFraction => _cameraDepthFraction;
        public float MinRotationSpeed => _minRotationSpeed;
        public float MaxRotationSpeed => _maxRotationSpeed;
        public float PulseAmplitude => _pulseAmplitude;
        public float PulseFrequency => _pulseFrequency;
        public float HoverAmplitude => _hoverAmplitude;
        public float HoverFrequency => _hoverFrequency;
        public float SpawnRadiusFraction => _spawnRadiusFraction;
        public float DepartureRadiusMultiplier => _departureRadiusMultiplier;
    }
}
