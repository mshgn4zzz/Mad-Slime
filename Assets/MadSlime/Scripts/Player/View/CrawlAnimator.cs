using MadSlime.Player.Data;
using MadSlime.Player.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Player.View
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

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_mover, name, nameof(_mover));

            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));

            ArgumentGuard.CheckNotEmpty(_config.CrawlStretchCurve, name, nameof(_config.CrawlStretchCurve));

            ArgumentGuard.CheckAssigned(_deformer, name, nameof(_deformer));
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

        [Inject]
        public void Construct(PlayerConfig config)
        {
            _config = config;
            _frequency = config.MovementSquashFrequency;
        }

        public void SetUrgency(float urgency)
        {
            float clampedUrgency = Mathf.Clamp01(urgency);
            float multiplier = Mathf.Lerp(1f, _config.FinalSquashFrequencyMultiplier, clampedUrgency);

            _frequency = _config.MovementSquashFrequency * multiplier;
        }

        private void UpdateCrawlStrength(float deltaTime)
        {
            bool isMoving = _mover.Velocity.sqrMagnitude > RestStrengthEpsilon;
            float targetStrength;

            if (isMoving)
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
