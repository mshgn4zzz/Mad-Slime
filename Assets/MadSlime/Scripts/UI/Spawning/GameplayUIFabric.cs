using Game;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI
{
    public class GameplayUIFabric : MonoBehaviour
    {
        [SerializeField] private Button _pauseButton;
        [SerializeField] private PauseMenu _pauseMenuPrefab;

        private UiSpawner _uiSpawner;
        private GameplayOrchestrator _orchestrator;
        private PauseMenu _activePauseMenu;
        private SceneNavigator _sceneNavigator;

        [Inject]
        public void Construct(UiSpawner uiSpawner, GameplayOrchestrator orchestrator, SceneNavigator sceneNavigator)
        {
            _uiSpawner = uiSpawner;
            _orchestrator = orchestrator;
            _sceneNavigator = sceneNavigator;
        }

        private void Awake()
        {
            if (_uiSpawner == null)
            {
                throw new InvalidOperationException(
                    $"{name}: UiSpawner was not injected. Check that GameLifetimeScope registers UiSpawner and GameplayUIFabric.");
            }

            if (_orchestrator == null)
            {
                throw new InvalidOperationException(
                    $"{name}: GameplayOrchestrator was not injected. Check that GameLifetimeScope registers GameplayOrchestrator and GameplayUIFabric.");
            }

            if (_pauseButton == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PauseButton is not assigned. Drag a Button into the _pauseButton field.");
            }

            if (_pauseMenuPrefab == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PauseMenu prefab is not assigned. Drag the PauseMenu prefab into the _pauseMenuPrefab field.");
            }
        }

        private void OnEnable()
        {
            _pauseButton.onClick.AddListener(OnPauseButtonClick);
        }

        private void OnDisable()
        {
            _pauseButton.onClick.RemoveListener(OnPauseButtonClick);

            if (_activePauseMenu != null)
            {
                _activePauseMenu.CloseButtonClicked -= OnPauseMenuCloseClicked;
                _activePauseMenu.MenuButtonClicked -= OnPauseMenuMenuClicked;
                _activePauseMenu.ResetConfirmed -= OnResetConfirmed;
                _activePauseMenu.Closed -= OnPauseMenuClosed;
                _activePauseMenu = null;
            }
        }

        private void OnPauseButtonClick()
        {
            if (_activePauseMenu != null)
            {
                return;
            }

            _orchestrator.PauseByRequest();

            _activePauseMenu = _uiSpawner.Spawn(_pauseMenuPrefab, UiLayer.Popup);
            _activePauseMenu.Initialize(true, true);
            _activePauseMenu.Closed += OnPauseMenuClosed;
            _uiSpawner.Show(_activePauseMenu);

            _activePauseMenu.CloseButtonClicked += OnPauseMenuCloseClicked;
            _activePauseMenu.MenuButtonClicked += OnPauseMenuMenuClicked;
            _activePauseMenu.ResetConfirmed += OnResetConfirmed;
        }

        private void OnResetConfirmed()
        {
            _sceneNavigator.ResetProgressAndLoadMenuAsync().Forget();
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
            _activePauseMenu.ResetConfirmed -= OnResetConfirmed;
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
