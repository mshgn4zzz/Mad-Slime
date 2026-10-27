using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public abstract class SceneState<TMachine> : MonoBehaviour, ISceneState
        where TMachine : class, ISessionStateMachine
    {
        private SceneNavigator _navigator;
        private SceneStateMachine _sceneMachine;
        private CancellationTokenSource _sessionSource;
        private CancellationTokenSource _preparationSource;
        private bool _isSubscribed;
        private bool _isEntered;

        public event Action SceneSessionPrepared;

        public event Action SceneSessionStarted;

        public event Action Finished;

        public bool IsFinished { get; private set; }

        protected TMachine Machine { get; private set; }

        protected virtual void Awake()
        {
            if (Machine == null || _navigator == null || _sceneMachine == null)
            {
                throw new InvalidOperationException($"{name}: scene state dependencies were not injected.");
            }

            ValidateConfiguration();
        }

        protected virtual void OnEnable()
        {
            _sessionSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        }

        protected virtual void Start()
        {
            _navigator.RegisterSession(PrepareAsync, StartScene, FinishScene, _sessionSource.Token);
        }

        protected virtual void OnDisable()
        {
            if (_navigator != null)
            {
                _navigator.UnregisterSession(StartScene);
            }

            if (Machine != null)
            {
                FinishScene();
                Exit();
            }

            if (_sessionSource != null)
            {
                _sessionSource.Cancel();
                _sessionSource.Dispose();
                _sessionSource = null;
            }
        }

        public async UniTask PrepareAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsFinished = false;
            Subscribe();

            using (CancellationTokenSource source = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _sessionSource.Token))
            {
                _preparationSource = source;

                try
                {
                    ConfigureMachine();
                    await Machine.PrepareAsync(source.Token);
                    source.Token.ThrowIfCancellationRequested();
                    SceneSessionPrepared?.Invoke();
                }
                catch
                {
                    Exit();
                    throw;
                }
                finally
                {
                    _preparationSource = null;
                }
            }
        }

        public void Enter()
        {
            _isEntered = true;
            Machine.Start();
            SceneSessionStarted?.Invoke();
        }

        public void Exit()
        {
            if (IsFinished)
            {
                return;
            }

            IsFinished = true;
            _isEntered = false;
            _preparationSource?.Cancel();
            Machine.Stop();
            OnStopped();

            if (_isSubscribed)
            {
                UnsubscribeMachineEvents();
                _isSubscribed = false;
            }

            Finished?.Invoke();
        }

        protected void Initialize(TMachine machine, SceneNavigator navigator, SceneStateMachine sceneMachine)
        {
            ArgumentGuard.CheckNotNull(machine, nameof(machine));
            ArgumentGuard.CheckNotNull(navigator, nameof(navigator));
            ArgumentGuard.CheckNotNull(sceneMachine, nameof(sceneMachine));

            Machine = machine;
            _navigator = navigator;
            _sceneMachine = sceneMachine;
        }

        protected void RequestTransition(SceneId sceneId)
        {
            if (_isEntered == false || _navigator.IsTransitioning)
            {
                return;
            }

            switch (sceneId)
            {
                case SceneId.Menu:
                    _navigator.LoadMenuAsync().Forget();
                    break;

                case SceneId.Game:
                    _navigator.LoadGameAsync().Forget();
                    break;

                case SceneId.Fill:
                    _navigator.LoadFillAsync().Forget();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(sceneId), sceneId, "Unknown target scene.");
            }
        }

        protected abstract void ValidateConfiguration();

        protected abstract void ConfigureMachine();

        protected abstract void SubscribeMachineEvents();

        protected abstract void UnsubscribeMachineEvents();

        protected virtual void OnStopped()
        {
        }

        private void Subscribe()
        {
            if (_isSubscribed)
            {
                return;
            }

            SubscribeMachineEvents();
            _isSubscribed = true;
        }

        private void StartScene()
        {
            _sceneMachine.Start(this);
        }

        private void FinishScene()
        {
            _sceneMachine.Stop(this);
        }
    }
}
