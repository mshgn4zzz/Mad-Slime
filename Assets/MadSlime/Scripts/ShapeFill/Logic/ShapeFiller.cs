using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace ShapeFill
{
    [RequireComponent(typeof(CubeSpawner))]
    public class ShapeFiller : MonoBehaviour
    {
        private enum BoostStage
        {
            Normal,
            Boosted,
            Instant
        }

        [SerializeField] private SpriteRenderer _ghostBackground;

        [SerializeField] private Vector3 _spawnPosition;

        private int _fillIndex;
        private int _arrivedCount;
        private int _currentTarget;
        private int _quotaTarget;
        private bool _isFilling;
        private bool _isStopped;
        private bool _hasResult;
        private BoostStage _boostStage;
        private Sprite _ghostSprite;
        private FillConfig _config;
        private CancellationTokenSource _fillCts;
        private CancellationTokenSource _borderCts;
        private GridBuilder _gridShape;
        private CubeSpawner _spawner;

        public int RequiredFillCount => _gridShape.FillCells.Count;

        public bool CanRescue()
        {
            return _hasResult == true && _isFilling == false && _currentTarget < RequiredFillCount;
        }

        public float GetFillFraction()
        {
            if (_currentTarget <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01(_arrivedCount / (float)_currentTarget);
        }

        public float GetFormFillFraction()
        {
            if (RequiredFillCount <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01(_arrivedCount / (float)RequiredFillCount);
        }

        public event Action<float> FillCompleted;

        public event Action<CubeFlightAnimator> CubeArrived;

        [Inject]
        public void Construct(GridBuilder gridShape, CubeSpawner spawner, FillConfig config)
        {
            _gridShape = gridShape;
            _spawner = spawner;
            _config = config;
        }

        private void Awake()
        {
            if (_gridShape == null)
            {
                throw new InvalidOperationException(
                    $"{name}: GridBuilder was not injected. Check that FillLifetimeScope "
                        + $"registers GridBuilder and ShapeFiller.");
            }

            if (_spawner == null)
            {
                throw new InvalidOperationException(
                    $"{name}: CubeSpawner was not injected. Check that FillLifetimeScope "
                        + $"registers CubeSpawner and ShapeFiller.");
            }

            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: FillConfig was not injected. Check that FillLifetimeScope has the FillConfig assigned.");
            }
        }

        private void OnDisable()
        {
            CancelFillLoop();
            CancelBorderCascade();
        }

        private void OnDestroy()
        {
            if (_ghostSprite != null)
            {
                Destroy(_ghostSprite);
                _ghostSprite = null;
            }
        }

        public void Initialize()
        {
            _spawner.Initialize();
        }

        public void BuildShape()
        {
            StopFill();
            _fillIndex = 0;
            _arrivedCount = 0;
            _currentTarget = 0;
            _quotaTarget = 0;
            _hasResult = false;

            _gridShape.Build();
            PlaceGhost();
            SpawnBorder();
        }

        public void ShowResult(FillResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result), $"{name}: ShowResult requires a calculated fill result.");
            }

            StopFill();
            _fillIndex = 0;
            _arrivedCount = 0;
            _quotaTarget = result.QuotaCubes;
            _currentTarget = result.TargetCubes;
            _boostStage = BoostStage.Normal;
            _hasResult = true;

            if (result.TargetCubes <= 0)
            {
                FillCompleted?.Invoke(0f);
                return;
            }

            _isFilling = true;
            _isStopped = false;
            StartFillLoop(token => FillAsync(token, _currentTarget));
        }

        public void Rescue()
        {
            if (CanRescue() == false)
            {
                throw new InvalidOperationException(
                    $"{name}: Rescue requires a finished fill below 100%. Check CanRescue before calling.");
            }

            _currentTarget = RequiredFillCount;
            _boostStage = BoostStage.Normal;
            _isFilling = true;
            _isStopped = false;
            StartFillLoop(token => FillAsync(token, _currentTarget));
        }

        public void StopFill()
        {
            _isFilling = false;
            _isStopped = true;
            CancelFillLoop();
        }

        public void Accelerate()
        {
            if (_isFilling == false)
            {
                return;
            }

            if (_boostStage == BoostStage.Normal)
            {
                _boostStage = BoostStage.Boosted;
                return;
            }

            if (_boostStage == BoostStage.Boosted)
            {
                StartInstantCompletion();
            }
        }

        private void StartInstantCompletion()
        {
            _boostStage = BoostStage.Instant;
            _isStopped = false;
            StartFillLoop(CompleteInstantlyAsync);
        }

        private void StartFillLoop(Func<CancellationToken, UniTaskVoid> loop)
        {
            CancelFillLoop();
            _fillCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            loop(_fillCts.Token).Forget();
        }

        private void CancelFillLoop()
        {
            if (_fillCts == null)
            {
                return;
            }

            _fillCts.Cancel();
            _fillCts.Dispose();
            _fillCts = null;
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
                Destroy(_ghostSprite);
            }

            _ghostSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                Vector2.one * 0.5f,
                texture.width / (_gridShape.Width * _gridShape.CellSize));

            _ghostBackground.sprite = _ghostSprite;
            _ghostBackground.color = new Color(1f, 1f, 1f, _config.GhostOpacity);
            _ghostBackground.transform.SetPositionAndRotation(_gridShape.GetTextureCenter(), _gridShape.transform.rotation);
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

            _borderCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

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

                await PumpFillAsync(cancellationToken, fillCells, 0, mainTarget);

                if (mainTarget < target)
                {
                    await SpawnBonusWaveAsync(cancellationToken, fillCells, mainTarget, target);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _isFilling = false;
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
                    float maxDuration = _config.MaxFillDuration;

                    if (_boostStage == BoostStage.Boosted)
                    {
                        maxDuration = _config.MaxBoostedFillDuration;
                    }

                    float duration = Mathf.Max(0.001f, Mathf.Min(GetSpawnInterval() * target, maxDuration));

                    progress = Mathf.Min(1f, progress + (Time.deltaTime / duration));
                    return CubeSpawnSchedule.GetTargetCount(progress, 1f, target);
                },
                index => SpawnFillCube(fillCells[index], GetFlightDuration()));
        }

        private int GetMainWaveTarget(int target)
        {
            if (_quotaTarget > 0 && _quotaTarget < target)
            {
                return _quotaTarget;
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
            if (_boostStage == BoostStage.Boosted)
            {
                return _config.BoostedSpawnInterval;
            }

            return _config.SpawnInterval;
        }

        private float GetFlightDuration()
        {
            if (_boostStage == BoostStage.Boosted)
            {
                return _config.BoostedFlightDuration;
            }

            return _config.FlightDuration;
        }

        private Color GetCubeColor(Vector2Int cell)
        {
            Color color = _gridShape.GetPixelColor(cell.x, cell.y);

            if (_fillIndex >= _quotaTarget)
            {
                color = Color.Lerp(color, _config.BonusTintColor, _config.BonusTintStrength);
            }

            return color;
        }

        private async UniTaskVoid CompleteInstantlyAsync(CancellationToken cancellationToken)
        {
            IReadOnlyList<Vector2Int> fillCells = _gridShape.FillCells;

            if (_fillIndex >= _currentTarget)
            {
                _isFilling = false;
                return;
            }

            try
            {
                int startIndex = _fillIndex;
                int remaining = _currentTarget - startIndex;
                float elapsed = 0f;

                await PumpSpawnAsync(
                    cancellationToken,
                    startIndex,
                    _currentTarget,
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

            _isFilling = false;
        }

        private void SpawnFillCube(Vector2Int cell, float flightDuration)
        {
            CubeFlightAnimator fillCube = _spawner.Spawn(
                _spawnPosition,
                UnityEngine.Random.rotation,
                _gridShape.CellSize,
                GetCubeColor(cell));

            fillCube.Arrived += OnCubeArrived;

            fillCube.Launch(_gridShape.GridToWorld(cell.x, cell.y), flightDuration);
        }

        private void OnCubeArrived(CubeFlightAnimator cube)
        {
            cube.Arrived -= OnCubeArrived;

            if (_isStopped == true)
            {
                return;
            }

            _arrivedCount++;

            CubeArrived?.Invoke(cube);

            if (_arrivedCount >= _currentTarget)
            {
                _isFilling = false;

                FillCompleted?.Invoke((float)_currentTarget / RequiredFillCount);
            }
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
    }
}
