using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public abstract class SceneState<TMachine> : MonoBehaviour, ISceneState
        where TMachine : class, ISessionStateMachine
    {
        private SceneStateMachine _sceneMachine;
        private CancellationTokenSource _sessionSource;
        private CancellationTokenSource _preparationSource;
        private bool _isSubscribed;
        private bool _isEntered;

        public event Action SceneSessionPrepared;

        public event Action SceneSessionStarted;

        public event Action Finished;

        public bool IsFinished { get; private set; }

        public abstract SceneId SceneId { get; }

        protected TMachine Machine { get; private set; }

        protected virtual void Awake()
        {
            if (Machine == null || _sceneMachine == null)
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
            _sceneMachine.RegisterScene(this, gameObject.scene, _sessionSource.Token);
        }

        protected virtual void OnDisable()
        {
            try
            {
                if (_sceneMachine != null)
                {
                    _sceneMachine.UnregisterScene(this);
                }

                if (Machine != null)
                {
                    Exit();
                }
            }
            finally
            {
                if (_sessionSource != null)
                {
                    _sessionSource.Cancel();
                    _sessionSource.Dispose();
                    _sessionSource = null;
                }
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

            try
            {
                Machine.Stop();
            }
            finally
            {
                try
                {
                    OnStopped();
                }
                finally
                {
                    if (_isSubscribed)
                    {
                        _isSubscribed = false;
                        UnsubscribeMachineEvents();
                    }

                    Finished?.Invoke();
                }
            }
        }

        protected void Initialize(TMachine machine, SceneStateMachine sceneMachine)
        {
            ArgumentGuard.CheckNotNull(machine, nameof(machine));
            ArgumentGuard.CheckNotNull(sceneMachine, nameof(sceneMachine));

            Machine = machine;
            _sceneMachine = sceneMachine;
        }

        protected void RequestTransition(SceneId sceneId)
        {
            if (_isEntered == false || _sceneMachine.IsTransitioning || _sceneMachine.IsStopped)
            {
                return;
            }

            _sceneMachine.ChangeStateAsync(sceneId).Forget();
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
    }
}
