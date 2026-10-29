using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.ShapeFill.Data;
using MadSlime.ShapeFill.View;
using MadSlime.Skins.Logic;
using MadSlime.Validation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MadSlime.ShapeFill.Logic
{
    public class FillPlayerPresenter : IDisposable
    {
        private const float ReverseRotationProbability = 0.5f;

        private readonly SkinApplier _skinApplier;
        private readonly FillPlayerView _view;
        private readonly FillPlayerConfig _config;
        private bool _isPrepared;

        public FillPlayerPresenter(SkinApplier skinApplier, FillPlayerView view, FillPlayerConfig config)
        {
            ArgumentGuard.CheckNotNull(skinApplier, nameof(skinApplier));
            ArgumentGuard.CheckNotNull(view, nameof(view));
            ArgumentGuard.CheckNotNull(config, nameof(config));

            _skinApplier = skinApplier;
            _view = view;
            _config = config;
            _skinApplier.ModelChanged += OnModelChanged;
        }

        public void Start()
        {
            if (_isPrepared == false)
            {
                throw new InvalidOperationException("Fill player must be prepared before starting.");
            }

            _view.SetAnimating(true);
        }

        public async UniTask PrepareAsync(CancellationToken cancellationToken)
        {
            Stop();

            await UniTask.WaitUntil(() => _skinApplier.CurrentModel != null, cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            ArgumentGuard.CheckOrder(
                _config.MinRotationSpeed, _config.MaxRotationSpeed, _config.name, nameof(_config.MinRotationSpeed));

            Vector3 angularVelocity = new Vector3(GetRotationSpeed(), GetRotationSpeed(), GetRotationSpeed());

            _view.Setup(_skinApplier.CurrentModel, _config, Random.rotation, angularVelocity);
            _isPrepared = true;
        }

        public void Stop()
        {
            _view.SetAnimating(false);
            _isPrepared = false;
        }

        public void GetEmission(out Vector3 position, out Vector3 direction, out float departureDistance)
        {
            if (_isPrepared == false)
            {
                throw new InvalidOperationException("Fill player must be prepared before emitting cubes.");
            }

            direction = Random.onUnitSphere;
            position = _view.Center + (Random.insideUnitSphere * (_view.Radius * _config.SpawnRadiusFraction));
            departureDistance = _view.Radius * _config.DepartureRadiusMultiplier;
        }

        public void Dispose()
        {
            _skinApplier.ModelChanged -= OnModelChanged;
            Stop();
        }

        private float GetRotationSpeed()
        {
            float speed = Random.Range(_config.MinRotationSpeed, _config.MaxRotationSpeed);

            if (Random.value < ReverseRotationProbability)
            {
                speed = -speed;
            }

            return speed;
        }

        private void OnModelChanged(GameObject model)
        {
            if (_isPrepared)
            {
                _view.SetModel(model);
            }
        }
    }
}
