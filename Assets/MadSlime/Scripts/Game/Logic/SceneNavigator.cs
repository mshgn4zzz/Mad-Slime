using Cysharp.Threading.Tasks;
using Saves;
using Skins;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
namespace Game
{
    public class SceneNavigator : MonoBehaviour
    {
        private const float SessionRegistrationTimeoutSeconds = 10f;
        private const float SaveReadinessTimeoutSeconds = 10f;

        [Tooltip("Экран загрузки: полноэкранный Canvas, показывается на время перехода между сценами.")]
        [SerializeField] private Canvas _loadingScreenPrefab;

        private SceneId _currentSceneId;
        private bool _isInitialized;
        private bool _isTransitioning;
        private bool _isDailyShownThisSession;
        private SceneSessionRegistry _registry = new SceneSessionRegistry();
        private LoadingScreenController _loading;
        private SceneTransitionRunner _transitionRunner = new SceneTransitionRunner();

        public SceneId CurrentSceneId => _currentSceneId;

        private ISavesReadiness _readiness;
        private SkinInventory _skinInventory;
        private IPlayerProgressStorage _progressStorage;
        private ISaveConfirmation _saveConfirmation;

        [Inject]
        public void Construct(ISavesReadiness readiness, SkinInventory skinInventory,
            IPlayerProgressStorage progressStorage, ISaveConfirmation saveConfirmation)
        {
            _readiness = readiness;
            _skinInventory = skinInventory;
            _progressStorage = progressStorage;
            _saveConfirmation = saveConfirmation;
        }

        public bool IsTransitioning => _isTransitioning;

        public bool TryMarkDailyShown()
        {
            if (_isDailyShownThisSession)
            {
                return false;
            }

            _isDailyShownThisSession = true;
            return true;
        }

        public void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            if (TryResolveLoadedSceneId(out SceneId bootSceneId) == false)
            {
                throw new InvalidOperationException(
                    "SceneNavigator: no loaded scene resolves to a known SceneId (Menu, Game, Fill). " +
                    "The boot scene must be loaded before SceneNavigator is used.");
            }

            _currentSceneId = bootSceneId;
            _loading = new LoadingScreenController(_loadingScreenPrefab);
            _isInitialized = true;
        }

        public void RegisterSession(Func<CancellationToken, UniTask> preparation, Action sessionStart,
            Action sessionFinish, CancellationToken sessionToken)
        {
            if (preparation == null)
            {
                throw new ArgumentNullException(nameof(preparation));
            }

            if (sessionStart == null)
            {
                throw new ArgumentNullException(nameof(sessionStart));
            }

            if (sessionFinish == null)
            {
                throw new ArgumentNullException(nameof(sessionFinish));
            }

            if (_isTransitioning)
            {
                _registry.RegisterPending(preparation, sessionStart, sessionFinish, sessionToken);
                return;
            }

            StartInitialSessionAsync(preparation, sessionStart, sessionFinish, sessionToken).Forget();
        }

        public void UnregisterSession(Action sessionStart)
        {
            _registry.Unregister(sessionStart);
        }

        private async UniTaskVoid StartInitialSessionAsync(Func<CancellationToken, UniTask> preparation,
            Action sessionStart, Action sessionFinish, CancellationToken sessionToken)
        {
            try
            {
                EnsureInitialized();
                _loading.Show(transform);
                ApplyLoadingSkin();
                await WaitForSaveReadinessAsync(sessionToken);
                _skinInventory.Initialize();
                _loading.SetSkin(_skinInventory.SelectedSkinId);
                await UniTask.NextFrame(cancellationToken: sessionToken);
                await preparation.Invoke(sessionToken);
                await _loading.CompleteAsync(sessionToken);
                sessionToken.ThrowIfCancellationRequested();
                _registry.SetCurrentSession(sessionStart, sessionFinish);
                sessionStart.Invoke();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                _loading.Hide();
            }
        }

        private bool IsSaveReady()
        {
            return _readiness.IsReady;
        }

        private async UniTask WaitForSaveReadinessAsync(CancellationToken cancellationToken)
        {
            float waitStart = Time.realtimeSinceStartup;

            while (IsSaveReady() == false)
            {
                if (Time.realtimeSinceStartup - waitStart > SaveReadinessTimeoutSeconds)
                {
                    throw new InvalidOperationException(
                        "SceneNavigator: saves did not become ready within "
                            + $"{SaveReadinessTimeoutSeconds} seconds. The Yandex Games SDK probably "
                            + $"failed to initialize.");
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }

        public UniTask LoadMenuAsync()
        {
            return LoadAsync(SceneId.Menu);
        }

        public async UniTask ResetProgressAndLoadMenuAsync()
        {
            if (_isTransitioning == true)
            {
                return;
            }

            _isTransitioning = true;
            EnsureInitialized();

            try
            {
                _loading.Show(transform);
                ApplyLoadingSkin();
                CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();

                try
                {
                    await _saveConfirmation.ConfirmSavedAsync(cancellationToken);
                }
                finally
                {
                    if (cancellationToken.IsCancellationRequested == false)
                    {
                        _registry.FinishCurrentSession();
                        _progressStorage.ResetProgress();
                        _skinInventory.Initialize();
                        await TransitionAsync(SceneId.Menu);
                    }
                }
            }
            finally
            {
                _registry.ClearPending();
                _loading.Hide();
                _isTransitioning = false;
            }
        }

        public UniTask LoadGameAsync()
        {
            return LoadAsync(SceneId.Game);
        }

        public UniTask LoadFillAsync()
        {
            return LoadAsync(SceneId.Fill);
        }

        private async UniTask LoadAsync(SceneId targetSceneId)
        {
            if (_isTransitioning)
            {
                return;
            }

            EnsureInitialized();

            if (targetSceneId == _currentSceneId)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetSceneId),
                    targetSceneId,
                    "SceneNavigator: cannot load the scene that is already active.");
            }

            _isTransitioning = true;

            try
            {
                await TransitionAsync(targetSceneId);
            }
            finally
            {
                _registry.ClearPending();
                _loading.Hide();
                _isTransitioning = false;
            }
        }

        private async UniTask TransitionAsync(SceneId targetSceneId)
        {
            EnsureInitialized();
            _loading.Show(transform);
            ApplyLoadingSkin();
            _registry.FinishCurrentSession();
            CancellationToken cancellationToken = this.GetCancellationTokenOnDestroy();

            await UniTask.NextFrame(cancellationToken: cancellationToken);
            SceneId sourceSceneId = _currentSceneId;
            Scene targetScene = await _transitionRunner.LoadAdditiveAsync(sourceSceneId, targetSceneId,
                cancellationToken);
            float registrationWaitStart = Time.realtimeSinceStartup;

            while (_registry.HasPendingSession == false)
            {
                if (Time.realtimeSinceStartup - registrationWaitStart > SessionRegistrationTimeoutSeconds)
                {
                    throw new InvalidOperationException(
                        $"SceneNavigator: scene '{SceneTransitionRunner.GetSceneName(targetSceneId)}' did not "
                            + $"register a session within {SessionRegistrationTimeoutSeconds} seconds. Its "
                            + $"lifetime scope or orchestrator probably failed during initialization.");
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            await _registry.PendingPreparation.Invoke(_registry.PendingSessionToken);
            await _loading.CompleteAsync(cancellationToken);
            _registry.PendingSessionToken.ThrowIfCancellationRequested();
            await _transitionRunner.UnloadOutgoingAsync(sourceSceneId, targetScene);

            Action sessionStart = _registry.PendingSceneStart;
            _registry.PromotePendingToCurrent();
            _currentSceneId = targetSceneId;

            sessionStart?.Invoke();

            _loading.Hide();
        }

        private void ApplyLoadingSkin()
        {
            string skinId = SkinInventory.InitialSkinId;

            if (_readiness.IsReady == true)
            {
                skinId = _skinInventory.SelectedSkinId;
            }

            _loading.SetSkin(skinId);
        }

        public void ReportPreparationProgress(float progress)
        {
            if (_loading != null)
            {
                _loading.SetProgress(progress);
            }
        }

        private static bool TryResolveLoadedSceneId(out SceneId sceneId)
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.IsValid() == true && TryResolveSceneId(activeScene.name, out sceneId) == true)
            {
                return true;
            }

            int sceneCount = SceneManager.sceneCount;

            for (int index = 0; index < sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);

                if (scene.IsValid() == false)
                {
                    continue;
                }

                if (TryResolveSceneId(scene.name, out sceneId) == true)
                {
                    return true;
                }
            }

            sceneId = default;
            return false;
        }

        private static bool TryResolveSceneId(string sceneName, out SceneId sceneId)
        {
            switch (sceneName)
            {
                case "Menu":
                    sceneId = SceneId.Menu;
                    return true;

                case "Game":
                    sceneId = SceneId.Game;
                    return true;

                case "Fill":
                    sceneId = SceneId.Fill;
                    return true;

                default:
                    sceneId = default;
                    return false;
            }
        }
    }
}
