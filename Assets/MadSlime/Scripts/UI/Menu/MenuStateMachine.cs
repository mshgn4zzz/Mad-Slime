using System;
using System.Threading;
using Audio;
using Cysharp.Threading.Tasks;
using Game;
using Roulette;
using UI.Shop;

namespace UI.Menu
{
    public class MenuStateMachine : BaseStateMachine<IMenuState>, ISessionStateMachine, IDisposable
    {
        private MenuRunningState _running;
        private MenuNavigation _navigation;
        private RoulettePreparation _roulettePreparation;
        private MenuContent _content;
        private Playlist _playlist;

        public MenuStateMachine(
            MenuRunningState running,
            MenuNavigation navigation,
            RoulettePreparation roulettePreparation,
            MenuContent content)
        {
            ArgumentGuard.CheckNotNull(running, nameof(running));
            ArgumentGuard.CheckNotNull(navigation, nameof(navigation));
            ArgumentGuard.CheckNotNull(roulettePreparation, nameof(roulettePreparation));
            ArgumentGuard.CheckNotNull(content, nameof(content));

            _running = running;
            _navigation = navigation;
            _roulettePreparation = roulettePreparation;
            _content = content;

            RegisterState(_running);
            _running.PlayRequested += OnPlayRequested;
        }

        public event Action PlayRequested;

        public void Configure(Playlist playlist)
        {
            ArgumentGuard.CheckNotNull(playlist, nameof(playlist));

            _playlist = playlist;
        }

        public UniTask PrepareAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_playlist == null)
            {
                throw new InvalidOperationException("MenuStateMachine must be configured before preparation.");
            }

            Stop();
            _navigation.Prepare();
            _roulettePreparation.Prepare();
            _content.Prepare();
            _running.Configure(_playlist);

            return UniTask.CompletedTask;
        }

        public void Start()
        {
            SwitchState<MenuRunningState>();
        }

        public void Dispose()
        {
            Stop();
            _running.PlayRequested -= OnPlayRequested;
        }

        private void OnPlayRequested()
        {
            if (CurrentState == _running)
            {
                PlayRequested?.Invoke();
            }
        }
    }
}
