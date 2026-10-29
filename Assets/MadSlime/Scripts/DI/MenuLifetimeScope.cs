using MadSlime.Ads.Logic;
using MadSlime.Economy.Logic;
using MadSlime.Game.Logic;
using MadSlime.GameFSM.Scenes;
using MadSlime.Roulette.Data;
using MadSlime.Roulette.Logic;
using MadSlime.Saves.Interfaces;
using MadSlime.Shop.Data;
using MadSlime.Shop.View;
using MadSlime.Skins.Data;
using MadSlime.UI.Menu;
using MadSlime.UI.Menu.PageStateMachine;
using MadSlime.UI.Shop;
using MadSlime.UI.Spawning;
using MadSlime.UI.Upgrades;
using MadSlime.UI.Roulette;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MadSlime.DI
{
    public class MenuLifetimeScope : ValidatedLifetimeScope
    {
        [SerializeField] private ShopItemView _shopItemViewPrefab;
        [SerializeField] private SkinRarityGroupView _rarityGroupPrefab;
        [SerializeField] private UpgradeItemView _upgradeItemViewPrefab;
        [SerializeField] private RouletteView _dailyRoulettePrefab;
        [SerializeField] private RouletteView _skinRoulettePrefab;
        [SerializeField] private MenuNavigation _navigation;
        [SerializeField] private MenuContent _content;
        [SerializeField] private ModelPlacer _modelPlacer;
        [SerializeField] private MenuSceneState _menuOrchestrator;
        [SerializeField] private RouletteConfig _rouletteConfig;
        [SerializeField] private ShopContent _shopContent;
        [SerializeField] private AdScheduler _adScheduler;
        [SerializeField] private Pauser _pauser;
        [SerializeField] private Transform _runtimeWindows;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateAssigned(_shopItemViewPrefab, nameof(_shopItemViewPrefab));
            ValidateAssigned(_rarityGroupPrefab, nameof(_rarityGroupPrefab));
            ValidateAssigned(_upgradeItemViewPrefab, nameof(_upgradeItemViewPrefab));
            ValidateAssigned(_dailyRoulettePrefab, nameof(_dailyRoulettePrefab));
            ValidateAssigned(_skinRoulettePrefab, nameof(_skinRoulettePrefab));
            ValidateAssigned(_navigation, nameof(_navigation));
            ValidateAssigned(_content, nameof(_content));
            ValidateAssigned(_modelPlacer, nameof(_modelPlacer));
            ValidateAssigned(_menuOrchestrator, nameof(_menuOrchestrator));
            ValidateAssigned(_rouletteConfig, nameof(_rouletteConfig));
            ValidateAssigned(_shopContent, nameof(_shopContent));
            ValidateAssigned(_adScheduler, nameof(_adScheduler));
            ValidateAssigned(_pauser, nameof(_pauser));
            ValidateAssigned(_runtimeWindows, nameof(_runtimeWindows));

            builder.RegisterComponent(_navigation);
            builder.RegisterComponent(_content);
            builder.RegisterComponentInHierarchy<SkinsPagePresenter>();
            builder.RegisterComponentInHierarchy<UpgradesPagePresenter>();
            builder.RegisterComponent(_modelPlacer);
            builder.Register<ShopItemViewFactory>(Lifetime.Scoped)
                .WithParameter(_shopItemViewPrefab)
                .WithParameter(_rarityGroupPrefab);
            builder.Register<UpgradeItemViewFactory>(Lifetime.Scoped)
                .WithParameter(_upgradeItemViewPrefab);
            builder.RegisterComponent(_menuOrchestrator);
            builder.Register<MenuPageStateMachine>(Lifetime.Scoped);
            builder.Register<MenuRunningState>(Lifetime.Scoped);
            builder.Register<MenuStateMachine>(Lifetime.Scoped);
            builder.Register<ResetProgressFlow>(Lifetime.Scoped);
            builder.Register<RouletteFactory>(resolver => new RouletteFactory(_rouletteConfig,
                resolver.Resolve<IRouletteStorage>(), resolver.Resolve<ISkinStorage>(),
                resolver.Resolve<SkinInventory>(), resolver.Resolve<Wallet>(),
                resolver.Resolve<SavesTransaction>()), Lifetime.Scoped);
            builder.Register<RoulettePreparation>(Lifetime.Scoped)
                .WithParameter("dailyRoulettePrefab", _dailyRoulettePrefab)
                .WithParameter("skinRoulettePrefab", _skinRoulettePrefab);
            builder.RegisterInstance(_shopContent);
            builder.RegisterInstance(_rouletteConfig);
            builder.RegisterComponent(_adScheduler);
            builder.Register<Wallet>(Lifetime.Scoped);
            builder.RegisterComponent(_pauser);

            builder.Register<UiSpawner>(Lifetime.Scoped);

            builder.RegisterBuildCallback(InitializeScene);
        }

        private void InitializeScene(IObjectResolver container)
        {
            container.Resolve<UiSpawner>().SetParent(_runtimeWindows);
            container.Resolve<Pauser>().ResetToPlay();

            SceneComponentInjector.Inject(container, gameObject.scene);
        }
    }
}
