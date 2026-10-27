using System;
using UnityEngine;
using VContainer;

namespace Player
{
    public class CrawlAnimator : MonoBehaviour
    {
        private const float RestStrengthEpsilon = 0.001f;

        [SerializeField] private Mover _mover;
        [SerializeField] private ModelDeformer _deformer;

        private PlayerConfig _config;
        private float _crawlPhase;
        private float _crawlStrength;
        private float _crawlStrengthVelocityRef;
        private float _frequency;

        [Inject]
        public void Construct(PlayerConfig config)
        {
            _config = config;
            _frequency = config.MovementSquashFrequency;
        }

        private void Awake()
        {
            if (_mover == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Mover is not assigned. Drag the Mover component into the _mover field.");
            }

            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerConfig was not injected. Check that GameLifetimeScope "
                        + $"registers PlayerConfig and CrawlAnimator.");
            }

            if (_config.CrawlStretchCurve == null || _config.CrawlStretchCurve.length == 0)
            {
                throw new InvalidOperationException(
                    "PlayerConfig has an empty CrawlStretchCurve. Add keyframes to the Crawl section.");
            }

            if (_deformer == null)
            {
                throw new InvalidOperationException($"{name}: assign the player ModelDeformer.");
            }
        }

        public void SetUrgency(float urgency)
        {
            float clampedUrgency = Mathf.Clamp01(urgency);
            float multiplier = Mathf.Lerp(1f, _config.FinalSquashFrequencyMultiplier, clampedUrgency);

            _frequency = _config.MovementSquashFrequency * multiplier;
        }

        private void OnDisable()
        {
            _deformer.SetMovementCompression(0f);
            _crawlStrength = 0f;
            _crawlStrengthVelocityRef = 0f;
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;

            if (deltaTime <= 0f)
            {
                return;
            }

            UpdateCrawlStrength(deltaTime);
            AdvanceCrawlPhase();
            ApplyDeform();
        }

        private void UpdateCrawlStrength(float deltaTime)
        {
            bool isMoving = _mover.Velocity.sqrMagnitude > RestStrengthEpsilon;
            float targetStrength;

            if (isMoving == true)
            {
                targetStrength = 1f;
            }
            else
            {
                targetStrength = 0f;
            }

            _crawlStrength = Mathf.Clamp01
            (
                Mathf.SmoothDamp(
                    _crawlStrength,
                    targetStrength,
                    ref _crawlStrengthVelocityRef,
                    _config.CrawlRampTime,
                    Mathf.Infinity,
                    deltaTime));
        }

        private void AdvanceCrawlPhase()
        {
            _crawlPhase = (_crawlPhase + (Time.deltaTime * _frequency)) % 1f;
        }

        private void ApplyDeform()
        {
            float cycleCompression = Mathf.Clamp01(_config.CrawlStretchCurve.Evaluate(_crawlPhase));
            float compression = _crawlStrength * _config.DeformMaxStretch * cycleCompression;

            _deformer.SetMovementCompression(compression);
        }
    }
}
