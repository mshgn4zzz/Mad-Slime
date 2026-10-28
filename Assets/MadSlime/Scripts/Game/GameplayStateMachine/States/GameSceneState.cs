using System;
using Audio;
using UnityEngine;
using VContainer;

namespace Game
{
    public class GameSceneState : SceneState<GameplayStateMachine>
    {
        [SerializeField] private Playlist _musicPlaylist;
        [SerializeField, Min(0f)] private float _endDelay = 1f;
        [SerializeField, Min(0.1f)] private float _finalSeconds = 20f;

        private SceneStateMachine _sceneMachine;
        private Pauser _pauser;

        public event Action GameplayStarted;
        public event Action SessionFinished;
        public event Action MenuRequested;
        public event Action FillRequested;

        public override SceneId SceneId => SceneId.Game;

        [Inject]
        public void Construct(GameplayStateMachine machine, SceneStateMachine sceneMachine, Pauser pauser)
        {
            ArgumentGuard.CheckNotNull(pauser, nameof(pauser));

            Initialize(machine, sceneMachine);
            _sceneMachine = sceneMachine;
            _pauser = pauser;
        }

        public void ExitToMenu()
        {
            MenuRequested?.Invoke();
            _pauser.ResetToPlay();
            RequestTransition(SceneId.Menu);
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
            if (_musicPlaylist == null)
            {
                throw new InvalidOperationException($"{name}: assign the game music playlist.");
            }
        }

        protected override void ConfigureMachine()
        {
            Machine.Configure(_musicPlaylist, _endDelay, _finalSeconds);
        }

        protected override void SubscribeMachineEvents()
        {
            Machine.GameplayStarted += OnGameplayStarted;
            Machine.SessionFinished += OnSessionFinished;
            Machine.FillRequested += OnFillRequested;
            Machine.PreparationProgressChanged += OnPreparationProgressChanged;
        }

        protected override void UnsubscribeMachineEvents()
        {
            Machine.GameplayStarted -= OnGameplayStarted;
            Machine.SessionFinished -= OnSessionFinished;
            Machine.FillRequested -= OnFillRequested;
            Machine.PreparationProgressChanged -= OnPreparationProgressChanged;
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
            FillRequested?.Invoke();
            _pauser.ResetToPlay();
            RequestTransition(SceneId.Fill);
        }

        private void OnPreparationProgressChanged(float progress)
        {
            _sceneMachine.ReportPreparationProgress(progress);
        }
    }
}
