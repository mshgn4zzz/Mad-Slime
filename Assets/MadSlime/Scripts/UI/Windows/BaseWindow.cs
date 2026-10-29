using System;
using MadSlime.UI.Animations;
using MadSlime.UI.Spawning;
using MadSlime.UI.Windows.StateMachine;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.UI.Windows
{
    [RequireComponent(typeof(ReactiveUIView))]
    public class BaseWindow : MonoBehaviour, IShowable
    {
        private ReactiveUIView _view;
        private WindowLifecycleStateMachine _machine;

        public event Action Closed;
        public event Action CloseRequested;

        protected virtual void Awake()
        {
            if (TryGetComponent(out _view) == false)
            {
                throw new InvalidOperationException($"{name}: ReactiveUIView is missing.");
            }

            ArgumentGuard.CheckAssigned(_machine, name, nameof(_machine));
            _machine.ShowRequested += OnShowRequested;
            _machine.CloseRequested += OnCloseRequested;
            _machine.HideRequested += OnHideRequested;
            _machine.Closed += OnClosed;
            _view.DisableCompleted += OnDisableCompleted;
        }

        protected virtual void OnDisable()
        {
            _machine?.Disable();
        }

        protected virtual void OnDestroy()
        {
            if (_machine == null)
            {
                return;
            }

            _view.DisableCompleted -= OnDisableCompleted;
            _machine.Dispose();
            _machine.ShowRequested -= OnShowRequested;
            _machine.CloseRequested -= OnCloseRequested;
            _machine.HideRequested -= OnHideRequested;
            _machine.Closed -= OnClosed;
        }

        [Inject]
        public void ConstructLifecycle(WindowLifecycleStateMachine machine)
        {
            _machine = machine;
        }

        public virtual void Show()
        {
            _machine.Show();
        }

        public void Hide()
        {
            BeginClose();
        }

        public void RequestClose()
        {
            _machine.RequestClose();
        }

        public void BeginClose()
        {
            _machine.BeginClose();
        }

        protected bool IsClosing()
        {
            return _machine != null && _machine.IsClosing();
        }

        protected virtual void OnClosing()
        {
        }

        private void OnShowRequested()
        {
            gameObject.SetActive(true);
            _view.Enable();
        }

        private void OnCloseRequested()
        {
            CloseRequested?.Invoke();
        }

        private void OnHideRequested()
        {
            OnClosing();
            _view.Disable();
        }

        private void OnDisableCompleted(ReactiveUIView view)
        {
            if (_machine.IsClosing())
            {
                _machine.NotifyHidden();
            }
        }

        private void OnClosed()
        {
            Closed?.Invoke();
        }
    }
}
