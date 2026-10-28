using Ads;
using Economy;
using Game;
using Roulette;
using Saves;
using Shop;
using Skins;
using UI.Menu;
using UI.Shop;
using UI.Spawning;
using UI.Upgrades;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class MenuLifetimeScope : ValidatedLifetimeScope
    {
        [SerializeField] private MenuNavigation _navigation;
        [SerializeField] private MenuContent _content;
        [SerializeField] private ModelPlacer _modelPlacer;
        [SerializeField] private ShopItemViewFactory _skinFactory;
        [SerializeField] private UpgradeItemViewFactory _upgradeFactory;
        [SerializeField] private MenuSceneState _menuOrchestrator;
        [SerializeField] private RouletteConfig _rouletteConfig;
        [SerializeField] private ShopContent _shopContent;
        [SerializeField] private RoulettePreparation _roulettePreparation;
        [SerializeField] private AdScheduler _adScheduler;
        [SerializeField] private Wallet _wallet;
        [SerializeField] private Pauser _pauser;
        [SerializeField] private Transform _runtimeWindows;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateAssigned(_navigation, nameof(_navigation));
            ValidateAssigned(_content, nameof(_content));
            ValidateAssigned(_modelPlacer, nameof(_modelPlacer));
            ValidateAssigned(_skinFactory, nameof(_skinFactory));
            ValidateAssigned(_upgradeFactory, nameof(_upgradeFactory));
            ValidateAssigned(_menuOrchestrator, nameof(_menuOrchestrator));
            ValidateAssigned(_rouletteConfig, nameof(_rouletteConfig));
            ValidateAssigned(_shopContent, nameof(_shopContent));
            ValidateAssigned(_roulettePreparation, nameof(_roulettePreparation));
            ValidateAssigned(_adScheduler, nameof(_adScheduler));
            ValidateAssigned(_wallet, nameof(_wallet));
            ValidateAssigned(_pauser, nameof(_pauser));
            ValidateAssigned(_runtimeWindows, nameof(_runtimeWindows));

            builder.RegisterComponent(_navigation);
            builder.RegisterComponent(_content);
            builder.RegisterComponent(_modelPlacer);
            builder.RegisterComponent(_skinFactory);
            builder.RegisterComponent(_upgradeFactory);
            builder.RegisterComponent(_menuOrchestrator);
            builder.Register<MenuRunningState>(Lifetime.Scoped);
            builder.Register<MenuStateMachine>(Lifetime.Scoped);
            builder.Register<ResetProgressFlow>(Lifetime.Scoped);
            builder.Register<RouletteFactory>(resolver => new RouletteFactory(_rouletteConfig,
                resolver.Resolve<IRouletteStorage>(), resolver.Resolve<ISkinStorage>(),
                resolver.Resolve<SkinInventory>(), resolver.Resolve<Wallet>(),
                resolver.Resolve<SavesTransaction>()), Lifetime.Scoped);
            builder.RegisterComponent(_roulettePreparation);
            builder.RegisterInstance(_shopContent);
            builder.RegisterInstance(_rouletteConfig);
            builder.RegisterComponent(_adScheduler);
            builder.RegisterComponent(_wallet);
            builder.RegisterComponent(_pauser);

            builder.Register<UiSpawner>(Lifetime.Scoped);

            builder.RegisterBuildCallback(InitializeScene);
        }

        private void InitializeScene(IObjectResolver container)
        {
            container.Resolve<UiSpawner>().SetParent(_runtimeWindows);
            container.Resolve<Pauser>().ResetToPlay();

            SceneButtonSoundInjector.Inject(container, gameObject.scene);
        }

    }
}
