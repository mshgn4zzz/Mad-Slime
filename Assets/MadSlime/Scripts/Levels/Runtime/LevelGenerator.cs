using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Collectables;
using Items;
using Quota;
using Saves;
using Scriptables;
using Skills;
using UnityEngine;
using VContainer;

namespace Game
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private Transform _itemsRoot;
        [SerializeField] private MeshRenderer _floorRenderer;

        private LevelPlacementPlanner _placementPlanner = new LevelPlacementPlanner();
        private PropVariantsAssigner _variantsAssigner = new PropVariantsAssigner();

        private LevelItemsSpawner _itemsSpawner;
        private LevelConfigResolver _configResolver;
        private ILevelStorage _levelStorage;
        private QuotaBoard _quotaBoard;
        private QuotaGenerator _quotaGenerator;
        private TierTable _tierTable;
        private LayoutsLibrary _layoutsLibrary;
        private Movement.Clamper _clamper;
        private ItemCollector _itemCollector;
        private GameObject _floorBackground;

        public IReadOnlyList<Item> SpawnedItems => _itemsSpawner.ActiveItems;

        [Inject]
        public void Construct(LevelConfigResolver configResolver, ILevelStorage levelStorage,
            QuotaBoard quotaBoard, QuotaGenerator quotaGenerator, TierTable tierTable,
            LayoutsLibrary layoutsLibrary, Movement.Clamper clamper, ItemCollector itemCollector)
        {
            _configResolver = configResolver;
            _levelStorage = levelStorage;
            _quotaBoard = quotaBoard;
            _quotaGenerator = quotaGenerator;
            _tierTable = tierTable;
            _layoutsLibrary = layoutsLibrary;
            _clamper = clamper;
            _itemCollector = itemCollector;
        }

        private void Awake()
        {
            if (_itemsRoot == null)
            {
                throw new InvalidOperationException(
                    $"{name}: ItemsRoot is not assigned. Drag a Transform into the _itemsRoot field.");
            }

            if (_configResolver == null)
            {
                throw new InvalidOperationException(
                    $"{name}: dependencies were not injected. GameLifetimeScope must be the first object in the scene hierarchy.");
            }

            if (_floorRenderer == null)
            {
                throw new InvalidOperationException(
                    $"{name}: FloorRenderer is not assigned. Drag a MeshRenderer into the _floorRenderer field.");
            }

            if (_clamper == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Clamper was not injected. Check that GameLifetimeScope registers Clamper and LevelGenerator.");
            }

            if (_quotaBoard == null)
            {
                throw new InvalidOperationException(
                    $"{name}: QuotaBoard was not injected. Check that ProjectLifetimeScope registers QuotaBoard and LevelGenerator.");
            }

            if (_itemCollector == null)
            {
                throw new InvalidOperationException(
                    $"{name}: ItemCollector was not injected. Check that GameLifetimeScope registers ItemCollector and LevelGenerator.");
            }

            _itemsSpawner = new LevelItemsSpawner(new ItemFactory(), _itemsRoot);
        }

        private void OnEnable()
        {
            _itemsSpawner.Attach(_itemCollector);
        }

        private void OnDisable()
        {
            _itemsSpawner.Detach();
        }

        private void OnDestroy()
        {
            _itemsSpawner?.Clear();
            if (_floorBackground != null) Destroy(_floorBackground);
        }

        public Bounds GetFloorBounds()
        {
            if (_floorRenderer == null)
            {
                return default;
            }

            return _floorRenderer.bounds;
        }

        public async UniTask GenerateAsync(Action<float> progress, CancellationToken cancellationToken)
        {
            LevelConfig config = _configResolver.GetConfigFor(_levelStorage.CurrentLevel);
            LayoutSet layout = _placementPlanner.PickLayout(_layoutsLibrary);
            Bounds floorBounds = GetFloorBounds();
            Dictionary<Item, ItemDefinition> variants = _variantsAssigner.Assign(config);
            IReadOnlyList<LevelPlacement> placements = _placementPlanner.Plan(
                config, layout, variants, _tierTable, transform, floorBounds);

            _clamper.SetBounds(floorBounds);
            ApplyTheme(config);
            _itemsSpawner.Clear();
            await _itemsSpawner.SpawnAsync(placements, progress, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            Physics.SyncTransforms();

            List<QuotaEntry> quota = _quotaGenerator.Generate(_itemsSpawner.SpawnedCounts, config);
            _quotaBoard.Reset(quota);
        }

        private void ApplyTheme(LevelConfig config)
        {
            if (_floorRenderer == null || config.Theme == null || config.Theme.FloorMaterial == null)
            {
                return;
            }

            _floorRenderer.sharedMaterial = config.Theme.FloorMaterial;
            ApplyFloorBackground(config.Theme);
        }

        private void ApplyFloorBackground(LevelTheme theme)
        {
            if (!theme.ExtendFloorBackground)
            {
                if (_floorBackground != null) _floorBackground.SetActive(false);
                return;
            }

            if (_floorBackground == null)
            {
                MeshFilter source = _floorRenderer.GetComponent<MeshFilter>();
                if (source == null || source.sharedMesh == null)
                    throw new InvalidOperationException("Extended level background requires a floor mesh.");

                _floorBackground = new GameObject("ThemeFloorBackground");
                _floorBackground.layer = _floorRenderer.gameObject.layer;
                _floorBackground.transform.SetParent(_floorRenderer.transform, false);
                _floorBackground.transform.localScale = new Vector3(16f, 1f, 16f);
                _floorBackground.transform.position = _floorRenderer.transform.position - Vector3.up * 0.05f;
                _floorBackground.AddComponent<MeshFilter>().sharedMesh = source.sharedMesh;
                MeshRenderer renderer = _floorBackground.AddComponent<MeshRenderer>();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            // The original floor renderer remains the sole source of map bounds.
            _floorBackground.SetActive(true);
            _floorBackground.GetComponent<MeshRenderer>().sharedMaterial = theme.FloorMaterial;
        }
    }
}
