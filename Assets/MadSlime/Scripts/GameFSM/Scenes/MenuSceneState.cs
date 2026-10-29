using MadSlime.Audio.Data;
using MadSlime.Game.Logic;
using MadSlime.UI.Menu;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.GameFSM.Scenes
{
    public class MenuSceneState : SceneState<MenuStateMachine>
    {
        [SerializeField] private Playlist _musicPlaylist;

        private Pauser _pauser;

        [Inject]
        public void Construct(MenuStateMachine machine, SceneStateMachine sceneMachine, Pauser pauser)
        {
            ArgumentGuard.CheckNotNull(pauser, nameof(pauser));

            Initialize(machine, sceneMachine);
            _pauser = pauser;
        }

        protected override void ValidateConfiguration()
        {
            ArgumentGuard.CheckAssigned(_musicPlaylist, name, nameof(_musicPlaylist));
        }

        protected override void ConfigureMachine()
        {
            Machine.Configure(_musicPlaylist);
        }

        public override void Enter()
        {
            Machine.PlayRequested += OnPlayRequested;
            base.Enter();
        }

        public override void Exit()
        {
            try
            {
                base.Exit();
            }
            finally
            {
                Machine.PlayRequested -= OnPlayRequested;
            }
        }

        private void OnPlayRequested()
        {
            if (IsTransitionAllowed() == false)
            {
                return;
            }

            _pauser.ResetToPlay();
            RequestTransition<GameSceneState>();
        }
    }
}
