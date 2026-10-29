using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MadSlime.Game.Logic
{
    public class SceneTransitor
    {
        private LoadingScreenPresenter _loading;
        private string _targetName;
        private Scene _loadedScene;

        public SceneTransitor(LoadingScreenPresenter loading)
        {
            _loading = loading;
        }

        public bool IsStopped { get; private set; }

        public void BeginTransition()
        {
            if (IsStopped)
            {
                throw new InvalidOperationException("SceneTransitor is stopped.");
            }

            _loading.Show();
        }

        public async UniTask<Scene> LoadAsync(Scene source, string targetName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DisableAll<Canvas>(source);
            DisableAll<EventSystem>(source);
            DisableAll<AudioListener>(source);
            _targetName = targetName;
            _loadedScene = default;
            SceneManager.sceneLoaded += OnSceneLoaded;

            try
            {
                AsyncOperation operation = SceneManager.LoadSceneAsync(targetName, LoadSceneMode.Additive);

                await operation.ToUniTask(cancellationToken: cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }

            if (_loadedScene.isLoaded == false || SceneManager.SetActiveScene(_loadedScene) == false)
            {
                throw new InvalidOperationException($"Scene '{targetName}' could not be activated.");
            }

            return _loadedScene;
        }

        public async UniTask CompleteAsync(Scene outgoing, CancellationToken cancellationToken)
        {
            await _loading.CompleteAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (outgoing.IsValid())
            {
                await SceneManager.UnloadSceneAsync(outgoing).ToUniTask(cancellationToken: cancellationToken);
            }
        }

        public void EndTransition()
        {
            _loading.Hide();
        }

        public void Stop()
        {
            IsStopped = true;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _loading.Hide();
        }

        private void DisableAll<T>(Scene scene) where T : Behaviour
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (T component in root.GetComponentsInChildren<T>(true))
                {
                    component.enabled = false;
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == _targetName && mode == LoadSceneMode.Additive)
            {
                _loadedScene = scene;
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }
    }
}
