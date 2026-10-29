using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Game.Logic;
using MadSlime.GameFSM.Interfaces;
using MadSlime.StateMachine.Logic;
using MadSlime.Validation;
using UnityEngine.SceneManagement;

namespace MadSlime.GameFSM
{
    public class SceneStateMachine : BaseStateMachine<ISceneState>, IDisposable
    {
        private SceneTransitor _transitor;
        private GameStartup _startup;
        private IReadOnlyDictionary<Type, string> _scenes;
        private CancellationTokenSource _lifetime;
        private CancellationTokenSource _preparation;
        private CancellationTokenRegistration _shutdownRegistration;
        private CancellationTokenRegistration _sceneRegistration;
        private Scene _currentScene;
        private ISceneState _pendingState;
        private Scene _pendingScene;
        private CancellationToken _pendingToken;
        private Type _targetType;
        private bool _isStarted;

        public SceneStateMachine(
            SceneTransitor transitor,
            GameStartup startup,
            IReadOnlyDictionary<Type, string> scenes,
            CancellationToken shutdownToken)
        {
            _transitor = transitor;
            _startup = startup;
            _scenes = scenes;
            _lifetime = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
            _shutdownRegistration = shutdownToken.Register(Stop);
        }

        public bool IsTransitioning { get; private set; }
        public bool IsStopped { get; private set; }
        public Exception Failure { get; private set; }

        public void RegisterScene(ISceneState state, Scene scene, CancellationToken lifetimeToken)
        {
            RequireRunning();
            ArgumentGuard.CheckNotNull(state, nameof(state));

            Type type = state.GetType();

            if (scene.isLoaded == false || scene.name != _scenes[type])
            {
                throw new InvalidOperationException("Scene state does not belong to its configured scene.");
            }

            if (_isStarted && (IsTransitioning == false || type != _targetType || _pendingState != null))
            {
                throw new InvalidOperationException($"Unexpected registration of scene state '{type.Name}'.");
            }

            lifetimeToken.ThrowIfCancellationRequested();
            _pendingState = state;
            _pendingScene = scene;
            _pendingToken = lifetimeToken;

            if (_isStarted == false)
            {
                _isStarted = true;
                _targetType = type;
                StartInitialAsync().Forget(HandleTransitionException);
            }
        }

        public UniTask SwitchStateAsync<TTarget>() where TTarget : ISceneState
        {
            RequireRunning();

            if (IsTransitioning || CurrentState?.GetType() == typeof(TTarget))
            {
                return UniTask.CompletedTask;
            }

            return TransitionAsync(typeof(TTarget));
        }

        public UniTask RestartCurrentAsync()
        {
            RequireRunning();

            if (IsTransitioning)
            {
                return UniTask.CompletedTask;
            }

            return TransitionAsync(CurrentState?.GetType());
        }

        public override void Stop()
        {
            if (IsStopped)
            {
                return;
            }

            IsStopped = true;
            _sceneRegistration.Dispose();
            _lifetime.Cancel();
            _transitor.Stop();
            ReleasePreparation();

            ISceneState pending = _pendingState;

            if (ReferenceEquals(CurrentState, pending))
            {
                pending = null;
            }

            ClearPending();
            IsTransitioning = false;
            base.Stop();
            pending?.Exit();
        }

        public void HandleTransitionException(Exception exception)
        {
            if (exception is OperationCanceledException && _lifetime.IsCancellationRequested)
            {
                Stop();
                return;
            }

            Failure = exception;
            Stop();
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        public void Dispose()
        {
            Stop();
            _shutdownRegistration.Dispose();
            _lifetime.Dispose();
        }

        private async UniTask StartInitialAsync()
        {
            IsTransitioning = true;
            _transitor.BeginTransition();
            await _startup.InitializeAsync(_lifetime.Token);
            await UniTask.NextFrame(cancellationToken: _lifetime.Token);
            await PrepareAndEnterAsync(default);
        }

        private async UniTask TransitionAsync(Type targetType)
        {
            if (CurrentState == null)
            {
                throw new InvalidOperationException("A scene transition requires a running state.");
            }

            string sceneName = _scenes[targetType];

            IsTransitioning = true;
            _targetType = targetType;

            Scene outgoing = _currentScene;

            _transitor.BeginTransition();
            _sceneRegistration.Dispose();
            base.Stop();

            Scene incoming = await _transitor.LoadAsync(outgoing, sceneName, _lifetime.Token);

            await UniTask.WaitUntil(() => _pendingState != null, cancellationToken: _lifetime.Token)
                .Timeout(TimeSpan.FromSeconds(10));

            if (_pendingScene != incoming)
            {
                throw new InvalidOperationException("Registered state belongs to a different loaded scene.");
            }

            await PrepareAndEnterAsync(outgoing);
        }

        private async UniTask PrepareAndEnterAsync(Scene outgoing)
        {
            _preparation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token, _pendingToken);

            CancellationToken token = _preparation.Token;

            await _pendingState.PrepareAsync(token);
            await _transitor.CompleteAsync(outgoing, token);
            token.ThrowIfCancellationRequested();
            SwitchTo(_pendingState);
            _currentScene = _pendingScene;
            _sceneRegistration = _pendingToken.Register(Stop);
            ClearPending();
            ReleasePreparation();
            _transitor.EndTransition();
            IsTransitioning = false;
        }

        private void ReleasePreparation()
        {
            _preparation?.Dispose();
            _preparation = null;
        }

        private void ClearPending()
        {
            _pendingState = null;
            _pendingScene = default;
            _pendingToken = default;
        }

        private void RequireRunning()
        {
            if (IsStopped)
            {
                throw new InvalidOperationException("SceneStateMachine is stopped.", Failure);
            }

            _lifetime.Token.ThrowIfCancellationRequested();
        }
    }
}
