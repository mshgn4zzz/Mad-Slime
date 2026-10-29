using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MadSlime.Player.Data;
using MadSlime.Validation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MadSlime.Collectables.Logic
{
    public class Absorber : MonoBehaviour
    {
        private const float NegativeArcProbability = 0.5f;

        [SerializeField] private PlayerConfig _config;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));
            ArgumentGuard.CheckOrder(
                _config.MinAbsorbDuration, _config.MaxAbsorbDuration, name, nameof(_config.MinAbsorbDuration));
            ArgumentGuard.CheckOrder(
                _config.MinArcFraction, _config.MaxArcFraction, name, nameof(_config.MinArcFraction));
            ArgumentGuard.CheckOrder(_config.MinSpinSpeed, _config.MaxSpinSpeed, name, nameof(_config.MinSpinSpeed));
        }

        public async UniTask AbsorbAsync(Transform itemTransform, CancellationToken cancellationToken)
        {
            Vector3 startPosition = itemTransform.position;
            Vector3 startScale = itemTransform.localScale;
            float duration = CalculateDuration(startPosition, transform.position);
            Vector3 controlPosition = CalculateControlPosition(startPosition, transform.position);
            Vector3 spinAxis = Random.onUnitSphere;
            float spinSpeed = Random.Range(_config.MinSpinSpeed, _config.MaxSpinSpeed);
            Quaternion startRotation = itemTransform.rotation;
            float progress = 0f;
            UniTaskCompletionSource completion = new UniTaskCompletionSource();

            cancellationToken.ThrowIfCancellationRequested();

            Tween absorption = DOTween.To(
                () => progress,
                value =>
                {
                    progress = value;

                    float easedProgress = DOVirtual.EasedValue(0f, 1f, value, _config.AbsorbEase);

                    itemTransform.position = global::MadSlime.Geometry.BezierCurve.GetPoint(
                        startPosition, controlPosition, transform.position, easedProgress);
                    itemTransform.rotation = Quaternion.AngleAxis(spinSpeed * duration * value, spinAxis)
                        * startRotation;

                    float shrink = Mathf.InverseLerp(_config.ShrinkStart, 1f, value);

                    itemTransform.localScale = startScale * (1f - shrink);
                },
                1f,
                duration)
                .SetEase(Ease.Linear)
                .SetTarget(itemTransform)
                .SetLink(itemTransform.gameObject, LinkBehaviour.KillOnDisable);

            absorption.OnComplete(() => completion.TrySetResult());
            absorption.OnKill(() => completion.TrySetCanceled());

            try
            {
                await completion.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                absorption.Kill();
            }
        }

        private float CalculateDuration(Vector3 startPosition, Vector3 endPosition)
        {
            float distance = Vector3.Distance(startPosition, endPosition);

            return Mathf.Clamp(distance / _config.AbsorbSpeed, _config.MinAbsorbDuration, _config.MaxAbsorbDuration);
        }

        private Vector3 CalculateControlPosition(Vector3 startPosition, Vector3 endPosition)
        {
            Vector3 offset = endPosition - startPosition;
            float distance = offset.magnitude;

            if (distance < 0.001f)
            {
                return Vector3.Lerp(startPosition, endPosition, 0.5f);
            }

            Vector3 perpendicular = Vector3.Cross(offset / distance, Vector3.up);

            if (perpendicular.sqrMagnitude < 0.001f)
            {
                perpendicular = Vector3.right;
            }
            else
            {
                perpendicular.Normalize();
            }

            float side = 1f;

            if (Random.value < NegativeArcProbability)
            {
                side = -1f;
            }

            float arcMagnitude = distance * Random.Range(_config.MinArcFraction, _config.MaxArcFraction) * side;

            return Vector3.Lerp(startPosition, endPosition, 0.5f) + (perpendicular * arcMagnitude);
        }
    }
}
