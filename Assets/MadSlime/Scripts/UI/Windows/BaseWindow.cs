using System;
using UI.Animations;
using UI.Spawning;
using UnityEngine;
namespace UI.Windows
{
    [RequireComponent(typeof(ReactiveUIView))]
    public class BaseWindow : MonoBehaviour, IShowable
    {
        public event Action Closed;

        public event Action CloseRequested;

        private ReactiveUIView _view;
        private bool _isClosing;
        private bool _isCloseRequested;
        private bool _isClosed;

        protected bool IsClosing => _isClosing;

        protected virtual void Awake()
        {
            if (TryGetComponent(out _view) == false)
            {
                throw new InvalidOperationException(
                    $"{name}: ReactiveUIView is missing. Assign the window animators in its prefab.");
            }
        }

        public virtual void Show()
        {
            _view.DisableCompleted -= OnDisableCompleted;
            _isClosing = false;
            _isCloseRequested = false;
            _isClosed = false;
            gameObject.SetActive(true);
            _view.Enable();
        }

        public void Hide()
        {
            BeginClose();
        }

        public void RequestClose()
        {
            if (_isClosing == true || _isCloseRequested == true)
            {
                return;
            }

            _isCloseRequested = true;
            Action closeRequested = CloseRequested;
            closeRequested?.Invoke();
        }

        public void BeginClose()
        {
            if (_isClosing == true)
            {
                return;
            }

            _isClosing = true;
            OnClosing();

            _view.DisableCompleted += OnDisableCompleted;
            _view.Disable();
        }

        protected virtual void OnClosing()
        {
        }

        protected virtual void OnDisable()
        {
            if (_view != null)
            {
                _view.DisableCompleted -= OnDisableCompleted;
            }
        }

        private void OnDisableCompleted(ReactiveUIView view)
        {
            if (_isClosed == true)
            {
                return;
            }

            _isClosed = true;
            _view.DisableCompleted -= OnDisableCompleted;

            Action closed = Closed;
            closed?.Invoke();
        }
    }
}
