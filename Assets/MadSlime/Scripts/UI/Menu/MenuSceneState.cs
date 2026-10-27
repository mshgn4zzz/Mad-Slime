using System;
using System.Threading;
using Audio;
using Cysharp.Threading.Tasks;
using Game;
using Game.Interfaces;
using Roulette;
using UI.Shop;
using UnityEngine;
using VContainer;

namespace UI.Menu
{
    public class MenuSceneFsmState : MonoBehaviour, ISceneFsmState
    {
        [SerializeField] private MenuNavigation _navigation;
        [SerializeField] private RoulettePreparation _roulettePreparation;
        [SerializeField] private MenuContent _content;
        [SerializeField] private Playlist _musicPlaylist;

        private IMusicPlayer _musicPlayer;

        public event Action PlayRequested;

        private void Awake()
        {
            if (_navigation == null || _roulettePreparation == null || _content == null || _musicPlaylist == null)
            {
                throw new InvalidOperationException($"{name}: assign the menu subsystems and music playlist.");
            }

            if (_musicPlayer == null)
            {
                throw new InvalidOperationException($"{name}: menu dependencies were not injected.");
            }
        }

        public UniTask PrepareAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _navigation.Prepare();
            _roulettePreparation.Prepare();
            _content.Prepare();

            return UniTask.CompletedTask;
        }

        public void Enter()
        {
            _navigation.PlayRequested += OnPlayRequested;

            _musicPlayer.Play(_musicPlaylist);
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

        [Inject]
        public void Construct(IMusicPlayer musicPlayer)
        {
            _musicPlayer = musicPlayer;
        }
    }
}