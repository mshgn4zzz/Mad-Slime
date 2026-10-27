using System;
using Audio;
using Game;
using UnityEngine;
using VContainer;

namespace UI.Menu
{
    public class MenuSceneState : SceneState<MenuStateMachine>
    {
        [SerializeField] private Playlist _musicPlaylist;

        private Pauser _pauser;

        public event Action PlayRequested;

        [Inject]
        public void Construct(MenuStateMachine machine, SceneNavigator navigator, SceneStateMachine sceneMachine, Pauser pauser)
        {
            ArgumentGuard.CheckNotNull(pauser, nameof(pauser));

            Initialize(machine, navigator, sceneMachine);
            _pauser = pauser;
        }

        protected override void ValidateConfiguration()
        {
            if (_musicPlaylist == null)
            {
                throw new InvalidOperationException($"{name}: assign the menu music playlist.");
            }
        }

        protected override void ConfigureMachine()
        {
            Machine.Configure(_musicPlaylist);
        }

        protected override void SubscribeMachineEvents()
        {
            Machine.PlayRequested += OnPlayRequested;
        }

        protected override void UnsubscribeMachineEvents()
        {
            Machine.PlayRequested -= OnPlayRequested;
        }

        private void OnPlayRequested()
        {
            PlayRequested?.Invoke();
            _pauser.ResetToPlay();
            RequestTransition(SceneId.Game);
        }
    }
}
