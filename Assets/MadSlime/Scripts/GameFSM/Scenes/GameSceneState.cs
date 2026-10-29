using System;
using MadSlime.Audio.Data;
using MadSlime.Game.GameplayStateMachine.Logic;
using MadSlime.Game.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.GameFSM.Scenes
{
    public class GameSceneState : SceneState<GameplayStateMachine>
    {
        [SerializeField] private Playlist _musicPlaylist;
        [SerializeField, Min(0f)] private float _endDelay = 1f;
        [SerializeField, Min(0.1f)] private float _finalSeconds = 20f;

        private Pauser _pauser;

        public event Action GameplayStarted;
        public event Action SessionFinished;

        [Inject]
        public void Construct(GameplayStateMachine machine, SceneStateMachine sceneMachine, Pauser pauser)
        {
            ArgumentGuard.CheckNotNull(pauser, nameof(pauser));

            Initialize(machine, sceneMachine);
            _pauser = pauser;
        }

        public void ExitToMenu()
        {
            if (IsTransitionAllowed() == false)
            {
                return;
            }

            _pauser.ResetToPlay();
            RequestTransition<MenuSceneState>();
        }

        public void PauseByRequest()
        {
            Machine.PauseByRequest();
        }

        public void ResumeByRequest()
        {
            Machine.ResumeByRequest();
        }

        protected override void ValidateConfiguration()
        {
            ArgumentGuard.CheckAssigned(_musicPlaylist, name, nameof(_musicPlaylist));
        }

        protected override void ConfigureMachine()
        {
            Machine.Configure(_musicPlaylist, _endDelay, _finalSeconds);
        }

        protected override void Subscribe()
        {
            Machine.GameplayStarted += OnGameplayStarted;
            Machine.SessionFinished += OnSessionFinished;
            Machine.FillRequested += OnFillRequested;
        }

        protected override void Unsubscribe()
        {
            if (Machine == null)
            {
                return;
            }

            Machine.GameplayStarted -= OnGameplayStarted;
            Machine.SessionFinished -= OnSessionFinished;
            Machine.FillRequested -= OnFillRequested;
        }

        private void OnGameplayStarted()
        {
            GameplayStarted?.Invoke();
        }

        private void OnSessionFinished()
        {
            SessionFinished?.Invoke();
        }

        private void OnFillRequested()
        {
            if (IsTransitionAllowed() == false)
            {
                return;
            }

            _pauser.ResetToPlay();
            RequestTransition<FillSceneState>();
        }
    }
}
