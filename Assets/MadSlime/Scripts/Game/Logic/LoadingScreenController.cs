using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Game.View;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Game.Logic
{
    public class LoadingScreenController
    {
        private Canvas _prefab;
        private Transform _parent;
        private LoadingScreenFactory _factory = new LoadingScreenFactory();
        private LoadingScreenView _view;

        public LoadingScreenController(Canvas prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        public void Show()
        {
            if (_view == null)
            {
                _view = _factory.Create(_prefab, _parent);
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
            if (_view == null)
            {
                return;
            }

            DestroyInstance();
        }

        private void DestroyInstance()
        {
            UnityEngine.Object.Destroy(_view.gameObject);
            _view = null;
        }
    }
}
