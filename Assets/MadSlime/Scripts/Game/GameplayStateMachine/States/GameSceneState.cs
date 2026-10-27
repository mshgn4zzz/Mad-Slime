using System;
using System.Threading;
using Audio;
using Cysharp.Threading.Tasks;
using Game.Interfaces;
using UnityEngine;
using VContainer;

namespace Game
{
    public class GameSceneState : MonoBehaviour, ISceneState
    {
        [SerializeField] private Playlist _musicPlaylist;
        [SerializeField, Min(0f)] private float _endDelay = 1f;
        [SerializeField, Min(0.1f)] private float _finalSeconds = 20f;

        private GameplayStateMachine _stateMachine;
        private bool _isEntered;
        private CancellationTokenSource _preparationSource;

        public event Action SceneSessionPrepared;
        public event Action SceneSessionStarted;
        public event Action GameplayStarted;
        public event Action SessionFinished;
        public event Action<float> PreparationProgressChanged;
        public event Action MenuRequested;
        public event Action FillRequested;

        private void Awake()
        {
            if (_stateMachine == null || _musicPlaylist == null)
            {
                throw new InvalidOperationException($"{name}: assign the music playlist and inject GameplayStateMachine.");
            }
        }

        [Inject]
        public void Construct(GameplayStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public async UniTask PrepareAsync(CancellationToken cancellationToken)
        {
            _stateMachine.PreparationProgressChanged += OnPreparationProgressChanged;
            try
            {
                using (CancellationTokenSource preparationSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, this.GetCancellationTokenOnDestroy()))
                {
                    _preparationSource = preparationSource;
                    await _stateMachine.PrepareAsync(_musicPlaylist, _endDelay, _finalSeconds,
                        preparationSource.Token);
                }
                SceneSessionPrepared?.Invoke();
            }
            finally
            {
                _preparationSource = null;
                _stateMachine.PreparationProgressChanged -= OnPreparationProgressChanged;
            }
        }

        public void Enter()
        {
            if (_isEntered)
            {
                return;
            }

            _isEntered = true;
            _stateMachine.GameplayStarted += OnGameplayStarted;
            _stateMachine.SessionFinished += OnSessionFinished;
            _stateMachine.FillRequested += OnFillRequested;
            _stateMachine.Start();
            SceneSessionStarted?.Invoke();
        }

        public void Exit()
        {
            _isEntered = false;
            _preparationSource?.Cancel();
            _stateMachine.Stop();
            _stateMachine.GameplayStarted -= OnGameplayStarted;
            _stateMachine.SessionFinished -= OnSessionFinished;
            _stateMachine.FillRequested -= OnFillRequested;
        }

        public void ExitToMenu()
        {
            if (_isEntered)
            {
                MenuRequested?.Invoke();
            }
        }

        public void PauseByRequest() => _stateMachine.PauseByRequest();
        public void ResumeByRequest() => _stateMachine.ResumeByRequest();

        private void OnDisable()
        {
            if (_isEntered)
            {
                Exit();
            }
        }

        private void OnDestroy()
        {
            if (_stateMachine != null)
            {
                Exit();
            }
        }

        private void OnGameplayStarted() => GameplayStarted?.Invoke();
        private void OnSessionFinished() => SessionFinished?.Invoke();
        private void OnFillRequested() => FillRequested?.Invoke();
        private void OnPreparationProgressChanged(float progress) => PreparationProgressChanged?.Invoke(progress);
    }
}
