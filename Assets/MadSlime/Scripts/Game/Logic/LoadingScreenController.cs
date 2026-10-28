using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class LoadingScreenController
    {
        private const string ControllerName = "LoadingScreenController";

        private Canvas _prefab;
        private Transform _parent;
        private Canvas _instance;
        private LoadingScreenView _view;

        public LoadingScreenController(Canvas prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        public void Show()
        {
            if (_prefab == null)
            {
                throw new InvalidOperationException(
                    $"{ControllerName}: loading screen prefab is not assigned. Drag a loading screen Canvas "
                        + $"prefab into the _loadingScreenPrefab field of ProjectLifetimeScope.");
            }

            if (_instance != null)
            {
                throw new InvalidOperationException("A loading screen is already open.");
            }

            _instance = UnityEngine.Object.Instantiate(_prefab, _parent);

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
                throw new InvalidOperationException("No loading screen is open.");
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
