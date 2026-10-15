using System;
using System.Threading;
using Audio;
using Cysharp.Threading.Tasks;
using Game;
using Roulette;
using Scriptables;
using Shop;
using UnityEngine;
using VContainer;

namespace UI
{
    public class MenuOrchestrator : MonoBehaviour
    {
        private enum SessionState
        {
            Preparing,
            Running,
            Finished
        }

        [SerializeField] private MenuNavigation _navigation;
        [SerializeField] private RoulettePreparation _roulettePreparation;
        [SerializeField] private MenuContent _content;
        [SerializeField] private Playlist _musicPlaylist;

        private SessionState _state;
        private CancellationTokenSource _sessionSource;
        private IMusicPlayer _musicPlayer;
        private SceneNavigator _sceneNavigator;
        private Pauser _pauser;

        [Inject]
        public void Construct(IMusicPlayer musicPlayer, SceneNavigator sceneNavigator, Pauser pauser)
        {
            _musicPlayer = musicPlayer;
            _sceneNavigator = sceneNavigator;
            _pauser = pauser;
        }

        private void Awake()
        {
            if (_navigation == null || _roulettePreparation == null || _content == null || _musicPlaylist == null)
            {
                throw new InvalidOperationException($"{name}: assign the menu subsystems and music playlist.");
            }

            if (_musicPlayer == null || _sceneNavigator == null || _pauser == null)
            {
                throw new InvalidOperationException($"{name}: menu dependencies were not injected.");
            }
        }

        private void OnEnable()
        {
            _state = SessionState.Preparing;
            _sessionSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            _navigation.PlayRequested += OnPlayRequested;
        }

        private void OnDisable()
        {
            FinishSession();
            _sceneNavigator.UnregisterSession(RunSession);
            _navigation.PlayRequested -= OnPlayRequested;
            _sessionSource.Dispose();
            _sessionSource = null;
        }

        private void Start()
        {
            _sceneNavigator.RegisterSession(PrepareSessionAsync, RunSession, FinishSession, _sessionSource.Token);
        }

        private UniTask PrepareSessionAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _navigation.Prepare();
            _roulettePreparation.Prepare();
            _content.Prepare();
            return UniTask.CompletedTask;
        }

        private void RunSession()
        {
            _state = SessionState.Running;
            _musicPlayer.Play(_musicPlaylist);
            _navigation.Run();
        }

        private void FinishSession()
        {
            if (_state == SessionState.Finished)
            {
                return;
            }

            _state = SessionState.Finished;
            _sessionSource?.Cancel();
            _navigation.Stop();
        }

        private void OnPlayRequested()
        {
            if (_state != SessionState.Running || _sceneNavigator.IsTransitioning == true)
            {
                return;
            }

            _pauser.ResetToPlay();
            NavigateToGame().Forget();
        }

        private async UniTaskVoid NavigateToGame()
        {
            await _sceneNavigator.LoadGameAsync();
        }
    }
}
