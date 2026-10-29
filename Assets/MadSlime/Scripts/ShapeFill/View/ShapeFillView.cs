using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.ShapeFill.Data;
using MadSlime.ShapeFill.Logic;
using MadSlime.ShapeFill.Logic.PlaybackStateMachine;
using UnityEngine;

namespace MadSlime.ShapeFill.View
{
    public class ShapeFillView : IDisposable
    {
        private readonly ShapeFillPlaybackData _data;
        private readonly GridBuilder _gridShape;
        private readonly CubeSpawner _spawner;
        private readonly FillConfig _config;
        private readonly FillPlayerPresenter _player;
        private readonly HashSet<CubeFlightAnimator> _pendingCubes = new HashSet<CubeFlightAnimator>();
        private SpriteRenderer _ghostBackground;
        private Sprite _ghostSprite;
        private CancellationToken _lifetime;
        private CancellationTokenSource _fillCts;
        private CancellationTokenSource _borderCts;
        private float _spawnInterval;
        private float _flightDuration;
        private float _maximumDuration;

        public ShapeFillView(
            ShapeFillPlaybackData data,
            GridBuilder grid,
            CubeSpawner spawner,
            FillConfig config,
            FillPlayerPresenter player)
        {
            _data = data;
            _gridShape = grid;
            _spawner = spawner;
            _config = config;
            _player = player;
        }

        public event Action SpawningCompleted;
        public event Action<CubeFlightAnimator> CubeArrived;

        public int RequiredFillCount => _gridShape.FillCells.Count;

        public void Configure(SpriteRenderer ghost, CancellationToken lifetime)
        {
            _ghostBackground = ghost;
            _lifetime = lifetime;
        }

        public void Initialize()
        {
            _spawner.Initialize();
        }

        public void BuildShape()
        {
            StopPlayback();
            _gridShape.Build();
            PlaceGhost();
            SpawnBorder();
        }

        public void SetSpeed(FillPlaybackSpeed speed)
        {
            _spawnInterval = speed.SpawnInterval;
            _flightDuration = speed.FlightDuration;
            _maximumDuration = speed.MaximumDuration;
        }

        public void Play()
        {
            StartFillLoop(token => FillAsync(token, _data.Target));
        }

        public void CompleteInstantly()
        {
            StartFillLoop(CompleteInstantlyAsync);
        }

        public void StopPlayback()
        {
            CancelFillLoop();

            foreach (CubeFlightAnimator cube in _pendingCubes)
            {
                if (cube != null)
                {
                    cube.Arrived -= OnCubeArrived;
                }
            }

            _pendingCubes.Clear();
        }

        public void Disable()
        {
            StopPlayback();
            CancelBorderCascade();
        }

        public void Dispose()
        {
            Disable();

            if (_ghostSprite != null)
            {
                UnityEngine.Object.Destroy(_ghostSprite);
            }
        }

        private void StartFillLoop(Func<CancellationToken, UniTaskVoid> loop)
        {
            CancelFillLoop();
            _fillCts = CancellationTokenSource.CreateLinkedTokenSource(_lifetime);
            loop(_fillCts.Token).Forget();
        }

        private void CancelFillLoop()
        {
            CancellationTokenSource source = _fillCts;

            _fillCts = null;
            source?.Cancel();
            source?.Dispose();
        }

        private void PlaceGhost()
        {
            if (_ghostBackground == null)
            {
                return;
            }

            Texture2D texture = _gridShape.ShapeTexture;

            if (_ghostSprite != null)
            {
                UnityEngine.Object.Destroy(_ghostSprite);
            }

            _ghostSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                Vector2.one * 0.5f,
                texture.width / (_gridShape.Width * _gridShape.CellSize));

            _ghostBackground.sprite = _ghostSprite;
            _ghostBackground.color = new Color(1f, 1f, 1f, _config.GhostOpacity);
            _ghostBackground.transform.SetPositionAndRotation(
                _gridShape.GetTextureCenter(),
                _gridShape.transform.rotation);
            _ghostBackground.transform.localScale = Vector3.one;
        }

        private void SpawnBorder()
        {
            CancelBorderCascade();

            IReadOnlyList<Vector2Int> borderCells = _gridShape.BorderCells;

            if (borderCells.Count == 0)
            {
                return;
            }

            _borderCts = CancellationTokenSource.CreateLinkedTokenSource(_lifetime);

            SpawnBorderCascadeAsync(borderCells, _borderCts.Token).Forget();
        }

        private async UniTaskVoid SpawnBorderCascadeAsync(
            IReadOnlyList<Vector2Int> borderCells,
            CancellationToken cancellationToken)
        {
            try
            {
                float elapsed = 0f;

                await PumpSpawnAsync(
                    cancellationToken,
                    0,
                    borderCells.Count,
                    () =>
                    {
                        elapsed += Time.deltaTime;
                        return CubeSpawnSchedule.GetTargetCount(
                        elapsed, _config.BorderCascadeDuration, borderCells.Count);
                    },
                    index => SpawnBorderCube(borderCells[index]));
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        private void SpawnBorderCube(Vector2Int cell)
        {
            CubeFlightAnimator borderCube = _spawner.Spawn(
                _gridShape.GridToWorld(cell.x, cell.y),
                Quaternion.identity,
                _gridShape.CellSize,
                _config.BorderColor);

            borderCube.GrowIn();
        }

        private async UniTaskVoid FillAsync(CancellationToken cancellationToken, int target)
        {
            IReadOnlyList<Vector2Int> fillCells = _gridShape.FillCells;

            try
            {
                if (_config.FillDelay > 0f)
                {
                    await UniTask.Delay((int)(_config.FillDelay * 1000f), cancellationToken: cancellationToken);
                }

                int mainTarget = GetMainWaveTarget(target);

                await PumpFillAsync(cancellationToken, fillCells, _data.Spawned, mainTarget);

                if (mainTarget < target)
                {
                    await SpawnBonusWaveAsync(
                        cancellationToken,
                        fillCells,
                        Mathf.Max(_data.Spawned, mainTarget),
                        target);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested == false)
            {
                SpawningCompleted?.Invoke();
            }
        }

        private async UniTask SpawnBonusWaveAsync(
            CancellationToken cancellationToken,
            IReadOnlyList<Vector2Int> fillCells,
            int startIndex,
            int target)
        {
            await UniTask.Delay((int)(_config.BonusWaveDelay * 1000f), cancellationToken: cancellationToken);

            await PumpFillAsync(cancellationToken, fillCells, startIndex, target);
        }

        private async UniTask PumpFillAsync(
            CancellationToken cancellationToken,
            IReadOnlyList<Vector2Int> fillCells,
            int startIndex,
            int target)
        {
            if (startIndex >= target)
            {
                return;
            }

            float progress = startIndex / (float)target;

            await PumpSpawnAsync(
                cancellationToken,
                startIndex,
                target,
                () =>
                {
                    float maxDuration = _maximumDuration;

                    float duration = Mathf.Max(0.001f, Mathf.Min(GetSpawnInterval() * target, maxDuration));

                    progress = Mathf.Min(1f, progress + (Time.deltaTime / duration));
                    return CubeSpawnSchedule.GetTargetCount(progress, 1f, target);
                },
                index => SpawnFillCube(fillCells[index], GetFlightDuration()));
        }

        private int GetMainWaveTarget(int target)
        {
            if (_data.GetQuota() > 0 && _data.GetQuota() < target)
            {
                return _data.GetQuota();
            }

            return target;
        }

        private async UniTask PumpSpawnAsync(
            CancellationToken cancellationToken,
            int startIndex,
            int target,
            Func<int> getDue,
            Action<int> spawnAt)
        {
            int index = startIndex;

            while (index < target)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int due = getDue();

                while (index < due)
                {
                    spawnAt(index);
                    index++;
                }

                if (index < target)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }
        }

        private float GetSpawnInterval()
        {
            return _spawnInterval;
        }

        private float GetFlightDuration()
        {
            return _flightDuration;
        }

        private Color GetCubeColor(Vector2Int cell)
        {
            Color color = _gridShape.GetPixelColor(cell.x, cell.y);

            if (_data.Spawned >= _data.GetQuota())
            {
                color = Color.Lerp(color, _config.BonusTintColor, _config.BonusTintStrength);
            }

            return color;
        }

        private async UniTaskVoid CompleteInstantlyAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<Vector2Int> fillCells = _gridShape.FillCells;

            if (_data.Spawned >= _data.Target)
            {
                SpawningCompleted?.Invoke();
                return;
            }

            try
            {
                int startIndex = _data.Spawned;
                int remaining = _data.Target - startIndex;
                float elapsed = 0f;

                await PumpSpawnAsync(
                    cancellationToken,
                    startIndex,
                    _data.Target,
                    () =>
                    {
                        elapsed += Time.deltaTime;
                        return startIndex + CubeSpawnSchedule.GetTargetCount(
                        elapsed, _config.InstantFillDuration, remaining);
                    },
                    index => SpawnFillCube(fillCells[index], _config.InstantFlightDuration));
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested == false)
            {
                SpawningCompleted?.Invoke();
            }
        }

        private void SpawnFillCube(Vector2Int cell, float flightDuration)
        {
            _player.GetEmission(out Vector3 position, out Vector3 direction, out float departureDistance);

            CubeFlightAnimator fillCube = _spawner.Spawn(
                position,
                UnityEngine.Random.rotation,
                _gridShape.CellSize,
                GetCubeColor(cell));

            _pendingCubes.Add(fillCube);
            fillCube.Arrived += OnCubeArrived;

            fillCube.Launch(_gridShape.GridToWorld(cell.x, cell.y), direction, departureDistance, flightDuration);
            _data.Spawned++;
        }

        private void CancelBorderCascade()
        {
            if (_borderCts == null)
            {
                return;
            }

            _borderCts.Cancel();
            _borderCts.Dispose();
            _borderCts = null;
        }

        private void OnCubeArrived(CubeFlightAnimator cube)
        {
            cube.Arrived -= OnCubeArrived;

            if (_pendingCubes.Remove(cube))
            {
                CubeArrived?.Invoke(cube);
            }
        }
    }
}
