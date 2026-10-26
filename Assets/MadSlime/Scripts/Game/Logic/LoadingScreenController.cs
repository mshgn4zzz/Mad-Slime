using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
namespace Game
{
    public class LoadingScreenController
    {
        private const string ControllerName = "LoadingScreenController";

        private Canvas _prefab;
        private Canvas _instance;
        private LoadingScreenView _view;

        public LoadingScreenController(Canvas prefab)
        {
            _prefab = prefab;
        }

        public void Show(Transform parent)
        {
            if (_prefab == null)
            {
                throw new InvalidOperationException(
                    $"{ControllerName}: loading screen prefab is not assigned. Drag a loading screen Canvas "
                        + $"prefab into the _loadingScreenPrefab field of SceneNavigator.");
            }

            if (_instance != null)
            {
                DestroyInstance();
            }

            _instance = UnityEngine.Object.Instantiate(_prefab, parent);

            if (_instance.TryGetComponent(out _view) == false)
            {
                DestroyInstance();
                throw new InvalidOperationException(
                    $"{ControllerName}: the loading prefab must contain LoadingScreenView.");
            }
        }

        public void SetSkin(string skinId)
        {
            if (_view != null)
            {
                _view.SetSkin(skinId);
            }
        }

        public void SetProgress(float progress)
        {
            if (_view != null)
            {
                _view.SetProgress(progress);
            }
        }

        public UniTask CompleteAsync(CancellationToken cancellationToken)
        {
            if (_view == null)
            {
                return UniTask.CompletedTask;
            }

            return _view.CompleteAsync(cancellationToken);
        }

        public void Hide()
        {
            if (_instance == null)
            {
                return;
            }

            DestroyInstance();
        }

        private void DestroyInstance()
        {
            UnityEngine.Object.Destroy(_instance.gameObject);
            _instance = null;
            _view = null;
        }
    }
}
