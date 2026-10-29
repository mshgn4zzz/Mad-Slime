using System;
using MadSlime.Ads.Logic;
using MadSlime.Audio.Data;
using MadSlime.Game.Logic;
using MadSlime.GameFSM.Interfaces;
using MadSlime.ShapeFill.FillStateMachine.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.GameFSM.Scenes
{
    public class FillSceneState : SceneState<FillStateMachine>
    {
        [SerializeField] private Playlist _musicPlaylist;
        [SerializeField] private MeshRenderer _floorRenderer;

        private AdScheduler _adScheduler;
        private PauseRequests _pauseRequests;

        public event Action<int> Succeeded;
        public event Action<int, bool> Failed;
        public event Action<Exception> PreparationFailed;

        [Inject]
        public void Construct(
            FillStateMachine machine,
            SceneStateMachine sceneMachine,
            AdScheduler adScheduler,
            Pauser pauser)
        {
            ArgumentGuard.CheckNotNull(adScheduler, nameof(adScheduler));
            ArgumentGuard.CheckNotNull(pauser, nameof(pauser));

            Initialize(machine, sceneMachine);
            _adScheduler = adScheduler;
            _pauseRequests = new PauseRequests(pauser);
        }

        public void ReloadLevel()
        {
            ExitTo<GameSceneState>();
        }

        public void ExitToMenu()
        {
            ExitTo<MenuSceneState>();
        }

        public void RescueFill()
        {
            Machine.RequestRescue();
        }

        public void Accelerate()
        {
            Machine.Accelerate();
        }

        public void PauseByRequest()
        {
            if (IsFinished)
            {
                return;
            }

            _pauseRequests.Acquire();
        }

        public void ResumeByRequest()
        {
            _pauseRequests.Release();
        }

        public override void Exit()
        {
            if (IsFinished)
            {
                return;
            }

            _pauseRequests.ReleaseAll();

            base.Exit();
        }

        protected override void ValidateConfiguration()
        {
            ArgumentGuard.CheckAssigned(_musicPlaylist, name, nameof(_musicPlaylist));
            ArgumentGuard.CheckAssigned(_floorRenderer, name, nameof(_floorRenderer));
        }

        protected override void ConfigureMachine()
        {
            Machine.Configure(_musicPlaylist, _floorRenderer);
        }

        private void ExitTo<TTarget>() where TTarget : ISceneState
        {
            if (IsTransitionAllowed() == false)
            {
                return;
            }

            _adScheduler.TryShowInterstitial();
            RequestTransition<TTarget>();
        }

        protected override void Subscribe()
        {
            Machine.Succeeded += OnWin;
            Machine.Failed += OnFailed;
            Machine.PreparationFailed += OnPreparationFailed;
        }

        protected override void Unsubscribe()
        {
            if (Machine == null)
            {
                return;
            }

            Machine.Succeeded -= OnWin;
            Machine.Failed -= OnFailed;
            Machine.PreparationFailed -= OnPreparationFailed;
        }

        private void OnWin(int reward)
        {
            Succeeded?.Invoke(reward);
        }

        private void OnFailed(int reward, bool isRescueAvailable)
        {
            Failed?.Invoke(reward, isRescueAvailable);
        }

        private void OnPreparationFailed(Exception exception)
        {
            PreparationFailed?.Invoke(exception);
        }
    }
}
