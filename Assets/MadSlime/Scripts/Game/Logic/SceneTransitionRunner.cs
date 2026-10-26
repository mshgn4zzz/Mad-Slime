using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
namespace Game
{
    public class SceneTransitionRunner
    {
        private const float ActivationProgressThreshold = 0.9f;

        public async UniTask<Scene> LoadAdditiveAsync(SceneId sourceSceneId, SceneId targetSceneId,
            CancellationToken cancellationToken)
        {
            Scene sourceScene = SceneManager.GetSceneByName(GetSceneName(sourceSceneId));

            if (sourceScene.isLoaded == true)
            {
                DisableOutgoingSystems(sourceScene);
            }

            string targetSceneName = GetSceneName(targetSceneId);
            void OnTargetSceneLoaded(Scene loadedScene, LoadSceneMode mode)
            {
                if (loadedScene.name == targetSceneName && sourceScene.isLoaded)
                {
                    GameObject[] roots = sourceScene.GetRootGameObjects();

                    for (int i = 0; i < roots.Length; i++)
                    {
                        DisableAll<AudioListener>(roots[i]);
                    }
                }
            }

            SceneManager.sceneLoaded += OnTargetSceneLoaded;

            try
            {
                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);

                if (loadOperation == null)
                {
                    throw new InvalidOperationException(
                        $"SceneTransitionRunner: failed to start loading scene '{targetSceneName}'. Check that "
                            + $"it is present in the build settings.");
                }

                loadOperation.allowSceneActivation = false;

                while (loadOperation.progress < ActivationProgressThreshold)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                loadOperation.allowSceneActivation = true;
                await loadOperation.ToUniTask(cancellationToken: cancellationToken);
            }
            finally
            {
                SceneManager.sceneLoaded -= OnTargetSceneLoaded;
            }

            Scene targetScene = SceneManager.GetSceneByName(targetSceneName);

            if (targetScene.isLoaded == false)
            {
                throw new InvalidOperationException(
                    $"SceneTransitionRunner: scene '{targetSceneName}' did not finish loading.");
            }

            SceneManager.SetActiveScene(targetScene);
            return targetScene;
        }

        public async UniTask UnloadOutgoingAsync(SceneId sourceSceneId, Scene targetScene)
        {
            Scene sourceScene = SceneManager.GetSceneByName(GetSceneName(sourceSceneId));

            if (sourceScene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(sourceScene).ToUniTask();
            }

            RestoreIncomingEventSystem(targetScene);
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
                    throw new ArgumentOutOfRangeException(
                        nameof(sceneId),
                        sceneId,
                        "SceneTransitionRunner: unknown scene id.");
            }
        }

        private static void DisableOutgoingSystems(Scene sourceScene)
        {
            GameObject[] roots = sourceScene.GetRootGameObjects();

            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                DisableAll<EventSystem>(roots[rootIndex]);
                DisableAll<Canvas>(roots[rootIndex]);
            }
        }

        private static void DisableAll<TComponent>(GameObject root)
        {
            TComponent[] components = root.GetComponentsInChildren<TComponent>(true);

            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                Behaviour behaviour = components[componentIndex] as Behaviour;

                if (behaviour != null)
                {
                    behaviour.enabled = false;
                }
            }
        }

        private static void RestoreIncomingEventSystem(Scene targetScene)
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject[] roots = targetScene.GetRootGameObjects();

            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                EventSystem[] eventSystems = roots[rootIndex].GetComponentsInChildren<EventSystem>(true);

                if (eventSystems.Length > 0)
                {
                    eventSystems[0].enabled = true;
                    return;
                }
            }
        }
    }
}
