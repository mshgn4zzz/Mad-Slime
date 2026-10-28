using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Game
{
    public class SceneNavigator
    {
        private bool _isLoading;

        public bool IsStopped { get; private set; }

        public async UniTask<Scene> LoadAsync(Scene source, SceneId targetId, CancellationToken cancellationToken)
        {
            try
            {
                RequireAvailable();
                _isLoading = true;
                cancellationToken.ThrowIfCancellationRequested();

                string targetName = GetSceneName(targetId);

                if (source.IsValid())
                {
                    DisableAll<Canvas>(source);
                    DisableAll<EventSystem>(source);
                    DisableAll<AudioListener>(source);
                }

                Scene target = default;
                UnityEngine.Events.UnityAction<Scene, LoadSceneMode> onLoaded = (scene, mode) =>
                {
                    if (scene.name == targetName && mode == LoadSceneMode.Additive)
                    {
                        target = scene;
                    }
                };

                SceneManager.sceneLoaded += onLoaded;

                try
                {
                    AsyncOperation operation = SceneManager.LoadSceneAsync(targetName, LoadSceneMode.Additive);

                    if (operation == null)
                    {
                        throw new InvalidOperationException($"Cannot start loading scene '{targetName}'.");
                    }

                    await operation.ToUniTask(cancellationToken: cancellationToken);
                }
                finally
                {
                    SceneManager.sceneLoaded -= onLoaded;
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (IsStopped)
                {
                    throw new InvalidOperationException("SceneNavigator is stopped.");
                }

                if (target.isLoaded == false || SceneManager.SetActiveScene(target) == false)
                {
                    throw new InvalidOperationException($"Scene '{targetName}' could not be activated.");
                }

                ValidateEventSystem(target);
                return target;
            }
            catch
            {
                Stop();
                throw;
            }
            finally
            {
                _isLoading = false;
            }
        }

        public async UniTask UnloadAsync(Scene scene)
        {
            try
            {
                RequireAvailable();

                if (scene.IsValid() == false || scene.isLoaded == false)
                {
                    throw new InvalidOperationException("The outgoing scene is not loaded.");
                }

                AsyncOperation operation = SceneManager.UnloadSceneAsync(scene);

                if (operation == null)
                {
                    throw new InvalidOperationException($"Cannot unload scene '{scene.name}'.");
                }

                await operation.ToUniTask();
                RequireAvailable();
            }
            catch
            {
                Stop();
                throw;
            }
        }

        public void Stop()
        {
            IsStopped = true;
        }

        public static string GetSceneName(SceneId sceneId)
        {
            switch (sceneId)
            {
                case SceneId.Menu:
                    return "Menu";

                case SceneId.Game:
                    return "Game";

                case SceneId.Fill:
                    return "Fill";

                default:
                    throw new ArgumentOutOfRangeException(nameof(sceneId), sceneId, "Unknown scene.");
            }
        }

        private void RequireAvailable()
        {
            if (IsStopped || _isLoading)
            {
                throw new InvalidOperationException("SceneNavigator is stopped or a load is already running.");
            }
        }

        private static void DisableAll<T>(Scene scene) where T : Behaviour
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (T component in root.GetComponentsInChildren<T>(true))
                {
                    component.enabled = false;
                }
            }
        }

        private static void ValidateEventSystem(Scene scene)
        {
            int count = 0;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (EventSystem eventSystem in root.GetComponentsInChildren<EventSystem>())
                {
                    if (eventSystem.isActiveAndEnabled)
                    {
                        count++;
                    }
                }
            }

            if (count != 1)
            {
                throw new InvalidOperationException($"Scene '{scene.name}' must have one active EventSystem; found {count}.");
            }
        }
    }
}
