using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MadSlime.Ads.Adapters;
using MadSlime.Ads.Interfaces;
using MadSlime.Ads.Logic.StateMachine;
using MadSlime.Audio.Logic;
using MadSlime.Game.Adapters;
using MadSlime.Game.Interfaces;
using MadSlime.Game.Logic;
using MadSlime.GameFSM;
using MadSlime.GameFSM.Scenes;
using MadSlime.Items.Data;
using MadSlime.Leaderboards.Adapters;
using MadSlime.Leaderboards.Interfaces;
using MadSlime.Levels.Data;
using MadSlime.Levels.Logic;
using MadSlime.Localization.Adapters;
using MadSlime.Localization.Interfaces;
using MadSlime.Localization.Logic;
using MadSlime.Quota.Logic;
using MadSlime.Saves.Adapters;
using MadSlime.Saves.Interfaces;
using MadSlime.Saves.Logic;
using MadSlime.Skins.Data;
using MadSlime.UI.Leaderboards.StateMachine;
using MadSlime.UI.Menu;
using MadSlime.UI.Windows.StateMachine;
using MadSlime.Upgrades.Logic;
using MadSlime.Upgrades.Data;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MadSlime.DI
{
    public class ProjectLifetimeScope : ValidatedLifetimeScope
    {
        [SerializeField] private UpgradesConfig _upgradesConfig;
        [SerializeField] private SaveConfirmation _saveConfirmation;
        [SerializeField] private Yg2LeaderboardService _leaderboardService;
        [SerializeField] private LevelsCatalog _levelsCatalog;
        [SerializeField] private LocalizationService _localizationService;
        [SerializeField] private TierTable _tierTable;
        [SerializeField] private LayoutsLibrary _layoutsLibrary;
        [SerializeField] private AudioPlayer _audioPlayer;
        [SerializeField] private AudioMixerController _audioMixerController;
        [SerializeField] private Canvas _loadingScreenPrefab;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateAssigned(_upgradesConfig, nameof(_upgradesConfig));
            ValidateAssigned(_saveConfirmation, nameof(_saveConfirmation));
            ValidateAssigned(_leaderboardService, nameof(_leaderboardService));
            ValidateAssigned(_levelsCatalog, nameof(_levelsCatalog));
            ValidateAssigned(_localizationService, nameof(_localizationService));
            ValidateAssigned(_tierTable, nameof(_tierTable));
            ValidateAssigned(_layoutsLibrary, nameof(_layoutsLibrary));
            ValidateAssigned(_audioPlayer, nameof(_audioPlayer));
            ValidateAssigned(_audioMixerController, nameof(_audioMixerController));
            ValidateAssigned(_loadingScreenPrefab, nameof(_loadingScreenPrefab));

            Yg2SavesAccess savesAccess = new Yg2SavesAccess(_saveConfirmation);
            SavesCore core = new SavesCore(savesAccess);
            BalanceStorage balanceStorage = new BalanceStorage(core);
            CollectedItemsStorage collectedItemsStorage = new CollectedItemsStorage(core);
            ProgressStorage progressStorage = new ProgressStorage(core, collectedItemsStorage);

            builder.RegisterInstance(savesAccess);
            builder.RegisterInstance(core);
            builder.RegisterInstance<SavesTransaction>(core.ExecuteTransaction);
            builder.RegisterInstance<ISaveConfirmation>(core);
            builder.RegisterInstance<ISavesReadiness>(core);
            builder.RegisterInstance<IBalanceStorage>(balanceStorage);
            builder.RegisterInstance<ICollectedItemsStorage>(collectedItemsStorage);
            builder.RegisterInstance<IAudioStorage>(new AudioStorage(core));
            builder.RegisterInstance<IUpgradesStorage>(new UpgradesStorage(core));
            builder.RegisterInstance<ISkinStorage>(new SkinsStorage(core));
            builder.RegisterInstance<IRouletteStorage>(new RouletteStorage(core));
            builder.RegisterInstance<ILanguageStorage>(new LanguageStorage(core));
            builder.RegisterInstance<ILevelStorage>(progressStorage);
            builder.RegisterInstance<IPlayerProgressStorage>(progressStorage);

            builder.RegisterInstance(_upgradesConfig);
            builder.Register<PlayerUpgrades>(Lifetime.Singleton);
            builder.RegisterBuildCallback(container => container.Resolve<PlayerUpgrades>());
            builder.Register<global::MadSlime.UI.Roulette.RouletteDisplayBuilder>(Lifetime.Singleton);
            builder.RegisterInstance(_localizationService.Translations);
            builder.RegisterComponent(_localizationService);
            builder.RegisterComponent(_audioPlayer)
                .As<global::MadSlime.Audio.Interfaces.IMusicPlayer>()
                .As<global::MadSlime.Audio.Interfaces.IGameSoundPlayer>()
                .As<global::MadSlime.Audio.Interfaces.IUISoundPlayer>();
            builder.RegisterComponent(_audioMixerController);
            builder.Register<SceneTransitor>(Lifetime.Singleton);
            builder.Register<LoadingScreenPresenter>(Lifetime.Singleton);
            builder.Register<GameStartup>(Lifetime.Singleton);
            builder.RegisterInstance<IReadOnlyDictionary<Type, string>>(new Dictionary<Type, string>
            {
                { typeof(MenuSceneState), "Menu" },
                { typeof(GameSceneState), "Game" },
                { typeof(FillSceneState), "Fill" },
            });
            builder.RegisterInstance(new LoadingScreenController(_loadingScreenPrefab, transform));
            builder.RegisterInstance(this.GetCancellationTokenOnDestroy());
            builder.Register<LeaderboardStateMachineFactory>(Lifetime.Singleton);
            builder.Register<WindowLifecycleStateMachine>(Lifetime.Transient);
            builder.Register<MenuSessionData>(Lifetime.Singleton);
            builder.RegisterInstance(_levelsCatalog);
            builder.RegisterInstance(_tierTable);
            builder.RegisterInstance(_layoutsLibrary);

            QuotaBoard quotaBoard = new QuotaBoard();

            builder.RegisterInstance(quotaBoard);
            builder.Register<QuotaCounter>(Lifetime.Singleton);
            builder.Register<LevelConfigResolver>(Lifetime.Singleton);
            builder.Register<SceneStateMachine>(Lifetime.Singleton);
            builder.Register<SkinInventory>(Lifetime.Singleton);
            builder.Register<RewardedAdStateMachine>(Lifetime.Singleton);
            builder.Register<IAdsService>(resolver => new Yg2AdsService(), Lifetime.Singleton);
            builder.RegisterComponent(_leaderboardService).As<ILeaderboardService>();
            builder.Register<ILanguageProvider>(resolver => new Yg2LanguageProvider(), Lifetime.Singleton);
            builder.Register<IGameVisibility, Yg2GameVisibility>(Lifetime.Singleton);
            builder.Register<IGameplayReporter>(resolver => new Yg2GameplayReporter(), Lifetime.Singleton);
        }
    }
}
