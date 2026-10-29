using MadSlime.Ads.Logic;
using MadSlime.Audio.View;
using MadSlime.Economy.Logic;
using MadSlime.Economy.Data;
using MadSlime.Game.Logic;
using MadSlime.GameFSM.Scenes;
using MadSlime.ShapeFill.Data;
using MadSlime.ShapeFill.FillStateMachine.Logic;
using MadSlime.ShapeFill.FillStateMachine.States;
using MadSlime.ShapeFill.Input;
using MadSlime.ShapeFill.Logic;
using MadSlime.ShapeFill.Logic.PlaybackStateMachine;
using MadSlime.ShapeFill.View;
using MadSlime.Skins.Logic;
using MadSlime.UI.Fill;
using MadSlime.UI.Fill.StateMachine;
using MadSlime.UI.Spawning;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MadSlime.DI
{
    public class FillLifetimeScope : ValidatedLifetimeScope
    {
        [SerializeField] private FillConfig _fillConfig;
        [SerializeField] private RewardConfig _rewardConfig;
        [SerializeField] private FillPlayerConfig _fillPlayerConfig;
        [SerializeField] private FillPlayerView _fillPlayerView;
        [SerializeField] private SkinApplier _skinApplier;
        [SerializeField] private FillSceneState _fillSceneOrchestrator;
        [SerializeField] private ShapeFillOrchestrator _fillOrchestrator;
        [SerializeField] private GridCameraFitter _cameraFitter;
        [SerializeField] private GridBuilder _gridBuilder;
        [SerializeField] private ShapeFiller _shapeFiller;
        [SerializeField] private CubeSpawner _cubeSpawner;
        [SerializeField] private FillTapButton _fillTapButton;
        [SerializeField] private FillCompleteAnimator _fillCompleteAnimator;
        [SerializeField] private FillCompleteSound _fillCompleteSound;
        [SerializeField] private FillProgressUI _fillProgressUI;
        [SerializeField] private AdScheduler _adScheduler;
        [SerializeField] private Pauser _pauser;

        [SerializeField] private FillUiPresenter _fillUiPresenter;
        [SerializeField] private FlyingCubeArrivalSound _flyingCubeArrivalSound;

        protected override void Configure(IContainerBuilder builder)
        {
            ValidateAssigned(_fillConfig, nameof(_fillConfig));
            ValidateAssigned(_rewardConfig, nameof(_rewardConfig));
            ValidateAssigned(_fillPlayerConfig, nameof(_fillPlayerConfig));
            ValidateAssigned(_fillPlayerView, nameof(_fillPlayerView));
            ValidateAssigned(_skinApplier, nameof(_skinApplier));
            ValidateAssigned(_fillSceneOrchestrator, nameof(_fillSceneOrchestrator));
            ValidateAssigned(_fillOrchestrator, nameof(_fillOrchestrator));
            ValidateAssigned(_cameraFitter, nameof(_cameraFitter));
            ValidateAssigned(_gridBuilder, nameof(_gridBuilder));
            ValidateAssigned(_shapeFiller, nameof(_shapeFiller));
            ValidateAssigned(_cubeSpawner, nameof(_cubeSpawner));
            ValidateAssigned(_fillTapButton, nameof(_fillTapButton));
            ValidateAssigned(_fillCompleteAnimator, nameof(_fillCompleteAnimator));
            ValidateAssigned(_fillCompleteSound, nameof(_fillCompleteSound));
            ValidateAssigned(_fillProgressUI, nameof(_fillProgressUI));
            ValidateAssigned(_adScheduler, nameof(_adScheduler));
            ValidateAssigned(_pauser, nameof(_pauser));
            ValidateAssigned(_fillUiPresenter, nameof(_fillUiPresenter));
            ValidateAssigned(_flyingCubeArrivalSound, nameof(_flyingCubeArrivalSound));

            builder.RegisterInstance(_fillConfig);
            builder.RegisterInstance(_rewardConfig);
            builder.RegisterInstance(_fillPlayerConfig);
            builder.RegisterComponent(_fillPlayerView);
            builder.RegisterComponent(_skinApplier);
            builder.Register<DoubleRewardModel>(Lifetime.Scoped);
            builder.Register<FillUiStateMachineFactory>(Lifetime.Scoped);
            builder.Register<ShapeFillPlaybackData>(Lifetime.Scoped);
            builder.Register<ShapeFillPlaybackStateMachine>(Lifetime.Scoped);
            builder.Register<ShapeFillView>(Lifetime.Scoped);
            builder.Register<FillPlayerPresenter>(Lifetime.Scoped);
            builder.Register<UiSpawner>(Lifetime.Scoped);
            builder.Register<FillSessionData>(Lifetime.Scoped);
            builder.Register<FillingState>(Lifetime.Scoped);
            builder.Register<ResultState>(Lifetime.Scoped);
            builder.Register<RescuingPrepareState>(Lifetime.Scoped);
            builder.Register<FillStateMachine>(Lifetime.Scoped);
            builder.RegisterComponent(_fillSceneOrchestrator);
            builder.RegisterComponent(_fillOrchestrator);
            builder.RegisterComponent(_cameraFitter);
            builder.RegisterComponent(_gridBuilder);
            builder.RegisterComponent(_shapeFiller);
            builder.RegisterComponent(_cubeSpawner);
            builder.Register<FillResultCalculator>(Lifetime.Scoped);
            builder.RegisterComponent(_fillTapButton);
            builder.RegisterComponent(_fillCompleteAnimator);
            builder.RegisterComponent(_fillCompleteSound);
            builder.RegisterComponent(_fillProgressUI);
            builder.Register<Rewarder>(Lifetime.Scoped);
            builder.RegisterComponent(_adScheduler);
            builder.Register<FillSessionSaver>(Lifetime.Scoped);
            builder.Register<LevelThemeApplier>(Lifetime.Scoped);
            builder.Register<Wallet>(Lifetime.Scoped);
            builder.RegisterComponent(_pauser);
            builder.RegisterBuildCallback(InjectSceneButtonSounds);
            builder.RegisterComponent(_fillUiPresenter);
            builder.RegisterComponent(_flyingCubeArrivalSound);
        }

        private void InjectSceneButtonSounds(IObjectResolver container)
        {
            SceneComponentInjector.Inject(container, gameObject.scene);
        }
    }
}
