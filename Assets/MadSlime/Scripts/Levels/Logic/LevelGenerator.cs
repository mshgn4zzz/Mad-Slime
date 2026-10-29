using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Collectables.Logic;
using MadSlime.Items.Data;
using MadSlime.Items.Logic;
using MadSlime.Items.View;
using MadSlime.Levels.Data;
using MadSlime.Player.Logic;
using MadSlime.Quota.Data;
using MadSlime.Quota.Logic;
using MadSlime.Saves.Interfaces;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Levels.Logic
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
        private Clamper _clamper;
        private ItemCollector _itemCollector;
        private LevelSurface _surface;
        private LevelEnvironment _environment;

        public IReadOnlyList<Item> SpawnedItems => _itemsSpawner.ActiveItems;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_itemsRoot, name, nameof(_itemsRoot));

            ArgumentGuard.CheckAssigned(_configResolver, name, nameof(_configResolver));

            ArgumentGuard.CheckAssigned(_floorRenderer, name, nameof(_floorRenderer));

            ArgumentGuard.CheckAssigned(_clamper, name, nameof(_clamper));

            ArgumentGuard.CheckAssigned(_quotaBoard, name, nameof(_quotaBoard));

            ArgumentGuard.CheckAssigned(_itemCollector, name, nameof(_itemCollector));

            _itemsSpawner = new LevelItemsSpawner(new ItemFactory(), _itemsRoot);

            if (_floorRenderer.TryGetComponent(out MeshFilter floorMesh) == false || floorMesh.sharedMesh == null)
            {
                throw new InvalidOperationException($"{name}: the floor must have a mesh.");
            }

            _surface = new LevelSurfaceFactory().Create(floorMesh.sharedMesh, transform);
            _environment = new LevelEnvironment(transform, new LevelEnvironmentFactory());
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
            if (_itemsSpawner != null)
            {
                _itemsSpawner.Clear();
            }

            if (_environment != null)
            {
                _environment.Clear();
            }
        }

        [Inject]
        public void Construct(
            LevelConfigResolver configResolver,
            ILevelStorage levelStorage,
            QuotaBoard quotaBoard,
            QuotaGenerator quotaGenerator,
            TierTable tierTable,
            LayoutsLibrary layoutsLibrary,
            Clamper clamper,
            ItemCollector itemCollector)
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
            _itemsSpawner.AssignQuotaLayers(quota);
        }

        private void ApplyTheme(LevelConfig config)
        {
            ArgumentGuard.CheckAssigned(config.Theme, config.name, nameof(config.Theme));

            ArgumentGuard.CheckAssigned(
                config.Theme.FloorMaterial, config.Theme.name, nameof(config.Theme.FloorMaterial));

            ArgumentGuard.CheckAssigned(_floorRenderer, name, nameof(_floorRenderer));

            _floorRenderer.sharedMaterial = config.Theme.FloorMaterial;
            _surface.Apply(config.Theme, GetFloorBounds());
            _environment.Apply(config.Theme, GetFloorBounds());
        }
    }
}
