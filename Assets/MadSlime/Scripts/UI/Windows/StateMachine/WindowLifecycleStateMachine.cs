using System;
using MadSlime.StateMachine.Logic;
using MadSlime.UI.Windows.StateMachine.States;

namespace MadSlime.UI.Windows.StateMachine
{
    public class WindowLifecycleStateMachine : BaseStateMachine<IWindowLifecycleState>, IDisposable
    {
        private readonly VisibleWindowState _visible = new VisibleWindowState();
        private readonly CloseRequestedWindowState _requested = new CloseRequestedWindowState();
        private readonly ClosingWindowState _closing = new ClosingWindowState();
        private readonly ClosedWindowState _closed = new ClosedWindowState();

        public WindowLifecycleStateMachine()
        {
            RegisterState(_visible);
            RegisterState(_requested);
            RegisterState(_closing);
            RegisterState(_closed);
            Subscribe();
        }

        public event Action ShowRequested;
        public event Action CloseRequested;
        public event Action HideRequested;
        public event Action Closed;

        public bool IsClosing()
        {
            return CurrentState == _closing || CurrentState == _closed;
        }

        public void Show()
        {
            Stop();
            SwitchState<VisibleWindowState>();
        }

        public void RequestClose()
        {
            if (CurrentState == _visible)
            {
                SwitchState<CloseRequestedWindowState>();
            }
        }

        public void BeginClose()
        {
            if (CurrentState == _visible || CurrentState == _requested)
            {
                SwitchState<ClosingWindowState>();
            }
        }

        public void NotifyHidden()
        {
            if (CurrentState == _closing)
            {
                SwitchState<ClosedWindowState>();
            }
        }

        public void Disable()
        {
            if (CurrentState != null && CurrentState != _closed)
            {
                SwitchState<ClosedWindowState>();
            }
        }

        public void Dispose()
        {
            Unsubscribe();
            base.Stop();
        }

        private void Subscribe()
        {
            _visible.Entered += OnShow;
            _requested.Entered += OnRequest;
            _closing.Entered += OnHide;
            _closed.Entered += OnClosed;
        }

        private void Unsubscribe()
        {
            _visible.Entered -= OnShow;
            _requested.Entered -= OnRequest;
            _closing.Entered -= OnHide;
            _closed.Entered -= OnClosed;
        }

        private void OnShow()
        {
            ShowRequested?.Invoke();
        }

        private void OnRequest()
        {
            CloseRequested?.Invoke();
        }

        private void OnHide()
        {
            HideRequested?.Invoke();
        }

        private void OnClosed()
        {
            Closed?.Invoke();
        }
    }
}
