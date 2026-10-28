using System;
using Ads;
using Audio;
using Cysharp.Threading.Tasks;
using Game;
using Items;
using Leaderboards;
using Levels;
using Localization;
using Quota;
using Saves;
using Skins;
using UnityEngine;
using UI.Menu;
using Upgrades;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class ProjectLifetimeScope : ValidatedLifetimeScope
    {
        [SerializeField] private PlayerUpgrades _playerUpgrades;
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
            ValidateAssigned(_playerUpgrades, nameof(_playerUpgrades));
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
            builder.RegisterInstance<Saves.ISavesReadiness>(core);
            builder.RegisterInstance<IBalanceStorage>(balanceStorage);
            builder.RegisterInstance<ICollectedItemsStorage>(collectedItemsStorage);
            builder.RegisterInstance<IAudioStorage>(new AudioStorage(core));
            builder.RegisterInstance<IUpgradesStorage>(new UpgradesStorage(core));
            builder.RegisterInstance<ISkinStorage>(new SkinsStorage(core));
            builder.RegisterInstance<IRouletteStorage>(new RouletteStorage(core));
            builder.RegisterInstance<ILanguageStorage>(new LanguageStorage(core));
            builder.RegisterInstance<ILevelStorage>(progressStorage);
            builder.RegisterInstance<IPlayerProgressStorage>(progressStorage);

            builder.RegisterComponent(_playerUpgrades);
            builder.RegisterComponent(_localizationService);
            builder.RegisterComponent(_audioPlayer)
                .As<Audio.IMusicPlayer>()
                .As<Audio.IGameSoundPlayer>()
                .As<Audio.IUISoundPlayer>();
            builder.RegisterComponent(_audioMixerController);
            builder.Register<SceneNavigator>(Lifetime.Singleton);
            builder.RegisterInstance(new LoadingScreenController(_loadingScreenPrefab, transform));
            builder.RegisterInstance(this.GetCancellationTokenOnDestroy());
            builder.Register<MenuSessionData>(Lifetime.Singleton);
            builder.RegisterInstance(_levelsCatalog);
            builder.RegisterInstance(_tierTable);
            builder.RegisterInstance(_layoutsLibrary);

            QuotaBoard quotaBoard = new QuotaBoard();
            QuotaCounter quotaCounter = new QuotaCounter(quotaBoard, _playerUpgrades);

            builder.RegisterInstance(quotaBoard);
            builder.RegisterInstance(quotaCounter);
            builder.Register<LevelConfigResolver>(Lifetime.Singleton);
            builder.Register<SceneStateMachine>(Lifetime.Singleton);
            builder.Register<SkinInventory>(Lifetime.Singleton);
            builder.Register<IAdsService>(resolver => new Yg2AdsService(), Lifetime.Singleton);
            builder.RegisterComponent(_leaderboardService).As<ILeaderboardService>();
            builder.Register<ILanguageProvider>(resolver => new Yg2LanguageProvider(), Lifetime.Singleton);
            builder.Register<IGameVisibility, Yg2GameVisibility>(Lifetime.Singleton);
            builder.Register<IGameplayReporter>(resolver => new Yg2GameplayReporter(), Lifetime.Singleton);
        }
    }
}
