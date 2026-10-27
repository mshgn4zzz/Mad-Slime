using System;
using Audio;
using Game;

namespace UI.Menu
{
    public class MenuRunningState : IMenuState
    {
        private MenuNavigation _navigation;
        private IMusicPlayer _musicPlayer;
        private Playlist _playlist;

        public MenuRunningState(MenuNavigation navigation, IMusicPlayer musicPlayer)
        {
            ArgumentGuard.CheckNotNull(navigation, nameof(navigation));
            ArgumentGuard.CheckNotNull(musicPlayer, nameof(musicPlayer));

            _navigation = navigation;
            _musicPlayer = musicPlayer;
        }

        public event Action PlayRequested;

        public void Configure(Playlist playlist)
        {
            ArgumentGuard.CheckNotNull(playlist, nameof(playlist));

            _playlist = playlist;
        }

        public void Enter()
        {
            if (_playlist == null)
            {
                throw new InvalidOperationException("MenuRunningState requires a playlist configured before entering.");
            }

            _navigation.PlayRequested += OnPlayRequested;
            _musicPlayer.Play(_playlist);
            _navigation.Run();
        }

        public void Exit()
        {
            _navigation.PlayRequested -= OnPlayRequested;
            _navigation.Stop();
        }

        private void OnPlayRequested()
        {
            PlayRequested?.Invoke();
        }
    }
}
