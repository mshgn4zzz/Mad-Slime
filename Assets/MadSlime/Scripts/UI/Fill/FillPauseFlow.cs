using System;
using ShapeFill;
using UI.Spawning;
using UI.Windows;

namespace UI.Fill
{
    public class FillPauseFlow
    {
        private UiSpawner _uiSpawner;
        private PauseMenu _prefab;
        private FillSceneState _orchestrator;
        private Func<bool> _canOpen;
        private Action<FillUiPresenter.WindowAction> _requestWindowAction;
        private Action _closeCompleted;
        private PauseMenu _activePauseMenu;
        private bool _hasPauseRequest;

        public FillPauseFlow(
            UiSpawner uiSpawner,
            PauseMenu prefab,
            FillSceneState orchestrator,
            Func<bool> canOpen,
            Action<FillUiPresenter.WindowAction> requestWindowAction,
            Action closeCompleted)
        {
            if (uiSpawner == null)
            {
                throw new ArgumentNullException(nameof(uiSpawner));
            }

            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (orchestrator == null)
            {
                throw new ArgumentNullException(nameof(orchestrator));
            }

            if (canOpen == null)
            {
                throw new ArgumentNullException(nameof(canOpen));
            }

            if (requestWindowAction == null)
            {
                throw new ArgumentNullException(nameof(requestWindowAction));
            }

            if (closeCompleted == null)
            {
                throw new ArgumentNullException(nameof(closeCompleted));
            }

            _uiSpawner = uiSpawner;
            _prefab = prefab;
            _orchestrator = orchestrator;
            _canOpen = canOpen;
            _requestWindowAction = requestWindowAction;
            _closeCompleted = closeCompleted;
        }

        public void Open()
        {
            if (_canOpen() == false || _activePauseMenu != null)
            {
                return;
            }

            _hasPauseRequest = true;
            _orchestrator.PauseByRequest();
            _activePauseMenu = _uiSpawner.Spawn(_prefab, UiLayer.Popup);
            _activePauseMenu.Initialize(false);
            _activePauseMenu.CloseButtonClicked += OnCloseButtonClicked;
            _activePauseMenu.MenuButtonClicked += OnMenuButtonClicked;
            _activePauseMenu.Closed += OnClosed;
            _uiSpawner.Show(_activePauseMenu);
        }

        public void Release()
        {
            if (_activePauseMenu == null)
            {
                return;
            }

            _activePauseMenu.CloseButtonClicked -= OnCloseButtonClicked;
            _activePauseMenu.MenuButtonClicked -= OnMenuButtonClicked;
            _activePauseMenu.Closed -= OnClosed;
            _activePauseMenu.BeginClose();
            _uiSpawner.Release(_activePauseMenu);
            _activePauseMenu = null;
            ResumeOwnedPause();
        }

        public void BeginClose(FillUiPresenter.WindowAction action)
        {
            if (_activePauseMenu == null)
            {
                return;
            }

            _activePauseMenu.CloseButtonClicked -= OnCloseButtonClicked;
            _activePauseMenu.MenuButtonClicked -= OnMenuButtonClicked;
            _requestWindowAction(action);
            _activePauseMenu.BeginClose();
        }

        private void OnCloseButtonClicked()
        {
            BeginClose(FillUiPresenter.WindowAction.None);
        }

        private void OnMenuButtonClicked()
        {
            BeginClose(FillUiPresenter.WindowAction.Menu);
        }

        private void OnClosed()
        {
            _activePauseMenu.Closed -= OnClosed;
            _uiSpawner.Release(_activePauseMenu);
            _activePauseMenu = null;
            ResumeOwnedPause();
            _closeCompleted();
        }

        private void ResumeOwnedPause()
        {
            if (_hasPauseRequest == false)
            {
                return;
            }

            _hasPauseRequest = false;
            _orchestrator.ResumeByRequest();
        }
    }
}
