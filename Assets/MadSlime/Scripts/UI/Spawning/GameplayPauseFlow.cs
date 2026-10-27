using System;
using Game;
using UI.Windows;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI.Spawning
{
    public class GameplayPauseFlow : MonoBehaviour
    {
        [SerializeField] private Button _pauseButton;
        [SerializeField] private PauseMenu _pauseMenuPrefab;

        private UiSpawner _uiSpawner;
        private GameSceneState _orchestrator;
        private PauseMenu _activePauseMenu;

        [Inject]
        public void Construct(UiSpawner uiSpawner, GameSceneState orchestrator)
        {
            _uiSpawner = uiSpawner;
            _orchestrator = orchestrator;
        }

        private void Awake()
        {
            if (_uiSpawner == null)
            {
                throw new InvalidOperationException(
                    $"{name}: UiSpawner was not injected. Check that GameLifetimeScope "
                        + $"registers UiSpawner and GameplayPauseFlow.");
            }

            if (_orchestrator == null)
            {
                throw new InvalidOperationException(
                    $"{name}: GameSceneState was not injected. Check that GameLifetimeScope "
                        + $"registers GameSceneState and GameplayPauseFlow.");
            }

            if (_pauseButton == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PauseButton is not assigned. Drag a Button into the _pauseButton field.");
            }

            if (_pauseMenuPrefab == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PauseMenu prefab is not assigned. Drag the PauseMenu prefab into "
                        + $"the _pauseMenuPrefab field.");
            }
        }

        private void OnEnable()
        {
            _pauseButton.onClick.AddListener(OnPauseButtonClick);
        }

        private void OnDisable()
        {
            _pauseButton.onClick.RemoveListener(OnPauseButtonClick);

            if (_activePauseMenu == null)
            {
                return;
            }

            _activePauseMenu.CloseButtonClicked -= OnPauseMenuCloseClicked;
            _activePauseMenu.MenuButtonClicked -= OnPauseMenuMenuClicked;
            _activePauseMenu.Closed -= OnPauseMenuClosed;

            if (_orchestrator != null)
            {
                _orchestrator.ResumeByRequest();
            }

            _uiSpawner.Release(_activePauseMenu);
            _activePauseMenu = null;
        }

        private void OnPauseButtonClick()
        {
            if (_activePauseMenu != null)
            {
                return;
            }

            _orchestrator.PauseByRequest();

            _activePauseMenu = _uiSpawner.Spawn(_pauseMenuPrefab, UiLayer.Popup);
            _activePauseMenu.Initialize(true);
            _activePauseMenu.Closed += OnPauseMenuClosed;
            _uiSpawner.Show(_activePauseMenu);

            _activePauseMenu.CloseButtonClicked += OnPauseMenuCloseClicked;
            _activePauseMenu.MenuButtonClicked += OnPauseMenuMenuClicked;
        }

        private void OnPauseMenuCloseClicked()
        {
            _activePauseMenu.CloseButtonClicked -= OnPauseMenuCloseClicked;
            _activePauseMenu.MenuButtonClicked -= OnPauseMenuMenuClicked;

            _orchestrator.ResumeByRequest();
            _activePauseMenu.BeginClose();
        }

        private void OnPauseMenuClosed()
        {
            _activePauseMenu.Closed -= OnPauseMenuClosed;
            _uiSpawner.Release(_activePauseMenu);
            _activePauseMenu = null;
        }

        private void OnPauseMenuMenuClicked()
        {
            _activePauseMenu.CloseButtonClicked -= OnPauseMenuCloseClicked;
            _activePauseMenu.MenuButtonClicked -= OnPauseMenuMenuClicked;

            _orchestrator.ResumeByRequest();
            _activePauseMenu.BeginClose();

            _orchestrator.ExitToMenu();
        }
    }
}
