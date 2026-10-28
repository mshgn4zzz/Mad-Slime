using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Saves;
using Skins;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class SceneStateMachine : BaseStateMachine<ISceneState>, IDisposable
    {
        private const float ReadinessTimeoutSeconds = 10f;
        private SceneNavigator _navigator;
        private LoadingScreenController _loading;
        private ISavesReadiness _readiness;
        private SkinInventory _inventory;
        private CancellationTokenSource _lifetime;
        private CancellationToken _shutdownToken;
        private Scene _currentScene;
        private ISceneState _pendingState;
        private Scene _pendingScene;
        private CancellationToken _pendingToken;
        private SceneId _targetId;
        private bool _hasStarted;
        private bool _awaitingRegistration;

        public SceneStateMachine(
            SceneNavigator navigator,
            LoadingScreenController loading,
            ISavesReadiness readiness,
            SkinInventory inventory,
            CancellationToken shutdownToken)
        {
            _navigator = navigator;
            _loading = loading;
            _readiness = readiness;
            _inventory = inventory;
            _shutdownToken = shutdownToken;
            _lifetime = CancellationTokenSource.CreateLinkedTokenSource(shutdownToken);
        }

        public bool IsTransitioning { get; private set; }

        public bool IsStopped { get; private set; }

        public Exception Failure { get; private set; }

        public SceneId CurrentSceneId => CurrentState.SceneId;

        public void RegisterScene(ISceneState state, Scene scene, CancellationToken lifetimeToken)
        {
            try
            {
                RequireRunning();
                ArgumentGuard.CheckNotNull(state, nameof(state));

                if (scene.isLoaded == false || scene.name != SceneNavigator.GetSceneName(state.SceneId))
                {
                    throw new InvalidOperationException("Scene state does not belong to its declared scene.");
                }

                if (_hasStarted == false)
                {
                    _hasStarted = true;
                    _targetId = state.SceneId;
                    IsTransitioning = true;
                    SetPending(state, scene, lifetimeToken);
                    StartInitialAsync().Forget();
                    return;
                }

                if (_awaitingRegistration == false || state.SceneId != _targetId || _pendingState != null)
                {
                    throw new InvalidOperationException($"Unexpected or duplicate registration of scene '{scene.name}'.");
                }

                SetPending(state, scene, lifetimeToken);
            }
            catch (Exception exception)
            {
                Fail(exception);
                throw;
            }
        }

        public void UnregisterScene(ISceneState state)
        {
            if (ReferenceEquals(CurrentState, state))
            {
                Stop();
            }
        }

        public UniTask ChangeStateAsync(SceneId target)
        {
            return TransitionAsync(target, false);
        }

        public UniTask RestartCurrentAsync()
        {
            return TransitionAsync(_targetId, true);
        }

        public void ReportPreparationProgress(float progress)
        {
            _loading.SetProgress(progress);
        }

        public override void Stop()
        {
            if (IsStopped)
            {
                return;
            }

            IsStopped = true;
            _navigator.Stop();
            _lifetime.Cancel();

            ISceneState preparing = _pendingState;

            if (ReferenceEquals(CurrentState, preparing))
            {
                preparing = null;
            }

            try
            {
                base.Stop();
            }
            finally
            {
                try
                {
                    preparing?.Exit();
                }
                finally
                {
                    ClearPending();
                    _loading.Hide();
                    IsTransitioning = false;
                }
            }
        }

        public void Fail(Exception exception)
        {
            if (Failure == null)
            {
                Failure = exception;
            }

            try
            {
                Stop();
            }
            catch (Exception cleanupException)
            {
                Debug.LogException(cleanupException);
            }
        }

        public void Dispose()
        {
            Stop();
            _lifetime.Dispose();
        }

        private async UniTask StartInitialAsync()
        {
            try
            {
                _loading.Show();
                _loading.SetSkin(SkinInventory.InitialSkinId);

                float started = Time.realtimeSinceStartup;

                while (_readiness.IsReady == false)
                {
                    CheckTimeout(started, "Saves did not become ready");
                    await UniTask.Yield(PlayerLoopTiming.Update, _lifetime.Token);
                }

                _inventory.Initialize();
                _loading.SetSkin(_inventory.SelectedSkinId);
                await UniTask.NextFrame(cancellationToken: _lifetime.Token);
                await PrepareAndEnterAsync(default);
            }
            catch (OperationCanceledException) when (_shutdownToken.IsCancellationRequested || (IsStopped && Failure == null))
            {
                Stop();
            }
            catch (Exception exception)
            {
                Fail(exception);
                throw;
            }
        }

        private async UniTask TransitionAsync(SceneId target, bool restart)
        {
            try
            {
                RequireRunning();

                if (IsTransitioning || CurrentState == null)
                {
                    throw new InvalidOperationException("A scene transition requires a running state and no other transition.");
                }

                SceneNavigator.GetSceneName(target);

                if (restart == false && target == CurrentSceneId)
                {
                    throw new InvalidOperationException("Use RestartCurrentAsync to restart the active scene.");
                }

                IsTransitioning = true;
                _targetId = target;

                Scene outgoing = _currentScene;

                _loading.Show();
                _loading.SetSkin(_inventory.SelectedSkinId);
                base.Stop();

                _awaitingRegistration = true;

                Scene incoming = await _navigator.LoadAsync(outgoing, target, _lifetime.Token);

                float started = Time.realtimeSinceStartup;

                while (_pendingState == null)
                {
                    CheckTimeout(started, $"Scene '{incoming.name}' did not register its state");
                    await UniTask.Yield(PlayerLoopTiming.Update, _lifetime.Token);
                }

                if (_pendingScene != incoming)
                {
                    throw new InvalidOperationException("Registered state belongs to a different loaded scene.");
                }

                _awaitingRegistration = false;
                await PrepareAndEnterAsync(outgoing);
            }
            catch (OperationCanceledException) when (_shutdownToken.IsCancellationRequested || (IsStopped && Failure == null))
            {
                Stop();
            }
            catch (Exception exception)
            {
                Fail(exception);
                throw;
            }
        }

        private async UniTask PrepareAndEnterAsync(Scene outgoing)
        {
            using (CancellationTokenSource preparation = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token, _pendingToken))
            {
                await _pendingState.PrepareAsync(preparation.Token);
                await _loading.CompleteAsync(preparation.Token);
                preparation.Token.ThrowIfCancellationRequested();

                if (outgoing.IsValid())
                {
                    await _navigator.UnloadAsync(outgoing);
                }

                preparation.Token.ThrowIfCancellationRequested();
                SwitchTo(_pendingState);
                _currentScene = _pendingScene;
                ClearPending();
                _loading.Hide();
                IsTransitioning = false;
            }
        }

        private void SetPending(ISceneState state, Scene scene, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            _pendingState = state;
            _pendingScene = scene;
            _pendingToken = token;
        }

        private void ClearPending()
        {
            _pendingState = null;
            _pendingScene = default;
            _pendingToken = default;
            _awaitingRegistration = false;
        }

        private void RequireRunning()
        {
            if (IsStopped)
            {
                throw new InvalidOperationException("SceneStateMachine is stopped.", Failure);
            }

            _lifetime.Token.ThrowIfCancellationRequested();
        }

        private static void CheckTimeout(float started, string message)
        {
            if (Time.realtimeSinceStartup - started > ReadinessTimeoutSeconds)
            {
                throw new InvalidOperationException($"{message} within {ReadinessTimeoutSeconds} seconds.");
            }
        }
    }
}
