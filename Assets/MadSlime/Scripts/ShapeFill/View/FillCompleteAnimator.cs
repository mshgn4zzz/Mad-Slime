using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MadSlime.Audio.View;
using MadSlime.ShapeFill.Data;
using MadSlime.ShapeFill.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.ShapeFill.View
{
    [RequireComponent(typeof(FillCompleteSound))]
    public class FillCompleteAnimator : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _confetti;

        private FillConfig _config;
        private ShapeFillOrchestrator _orchestrator;
        private GridBuilder _gridBuilder;
        private FillCompleteSound _completeSound;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_orchestrator, name, nameof(_orchestrator));

            ArgumentGuard.CheckAssigned(_gridBuilder, name, nameof(_gridBuilder));

            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));

            ArgumentGuard.CheckAssigned(_confetti, name, nameof(_confetti));

            if (_confetti.gameObject.scene.IsValid() == false)
            {
                throw new InvalidOperationException(
                    $"{name}: Confetti must be a scene instance, not a prefab asset. Drag the "
                    + $"scene object into the _confetti field.");
            }

            if (TryGetComponent(out _completeSound) == false)
            {
                throw new InvalidOperationException(
                    $"{name}: FillCompleteSound was not found. It is required by "
                    + $"FillCompleteAnimator and must sit on the same GameObject.");
            }

            _confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnEnable()
        {
            _orchestrator.FillCompleted += OnFillCompleted;
        }

        private void OnDisable()
        {
            _orchestrator.FillCompleted -= OnFillCompleted;
        }

        [Inject]
        public void Construct(ShapeFillOrchestrator orchestrator, GridBuilder gridBuilder, FillConfig config)
        {
            _orchestrator = orchestrator;
            _gridBuilder = gridBuilder;
            _config = config;
        }

        public void Play()
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Camera.main is not found. Tag the fill scene camera with the MainCamera tag.");
            }

            _completeSound.Play();
            PlayConfettiAsync().Forget();
            PlayShapePunch();
            PlayCameraKick(mainCamera);
        }

        private async UniTaskVoid PlayConfettiAsync()
        {
            CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();

            try
            {
                float duration = 0f;
                float maxLifetime = 0f;

                foreach (ParticleSystem system in _confetti.GetComponentsInChildren<ParticleSystem>(true))
                {
                    ParticleSystem.MainModule main = system.main;

                    duration = Mathf.Max(duration, main.duration);
                    maxLifetime = Mathf.Max(maxLifetime, main.startLifetime.constantMax);
                }

                _confetti.Play(true);

                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: cancellationToken);

                _confetti.Stop(true, ParticleSystemStopBehavior.StopEmitting);

                await UniTask.Delay(TimeSpan.FromSeconds(maxLifetime), cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        private void PlayShapePunch()
        {
            _gridBuilder.transform.DOKill();
            _gridBuilder.transform.DOPunchScale(
                Vector3.one * _config.ShapePunchStrength,
                _config.ShapePunchDuration,
                _config.ShapePunchVibrato,
                _config.ShapePunchElasticity)
                .SetLink(gameObject);
        }

        private void PlayCameraKick(Camera camera)
        {
            float startFov = camera.fieldOfView;

            camera.DOKill();

            Sequence kick = DOTween.Sequence().SetLink(gameObject);

            kick.Append(camera.DOFieldOfView(startFov + _config.FovKick, _config.FovDuration).SetEase(Ease.OutQuad));
            kick.Append(camera.DOFieldOfView(startFov, _config.FovDuration).SetEase(Ease.InQuad));
        }

        private void OnFillCompleted(float percent)
        {
            if (percent < 1f)
            {
                return;
            }

            Play();
        }
    }
}
