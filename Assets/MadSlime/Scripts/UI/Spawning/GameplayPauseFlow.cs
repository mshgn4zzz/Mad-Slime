using MadSlime.GameFSM.Scenes;
using MadSlime.UI.Windows;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MadSlime.UI.Spawning
{
    public class GameplayPauseFlow : MonoBehaviour
    {
        [SerializeField] private Button _pauseButton;
        [SerializeField] private PauseMenu _pauseMenuPrefab;

        private UiSpawner _uiSpawner;
        private GameSceneState _orchestrator;
        private PauseMenu _activePauseMenu;
        private PauseMenuSubscription _pauseSubscription;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_uiSpawner, name, nameof(_uiSpawner));

            ArgumentGuard.CheckAssigned(_orchestrator, name, nameof(_orchestrator));

            ArgumentGuard.CheckAssigned(_pauseButton, name, nameof(_pauseButton));

            ArgumentGuard.CheckAssigned(_pauseMenuPrefab, name, nameof(_pauseMenuPrefab));
        }

        private void OnEnable()
        {
            _pauseButton.onClick.AddListener(OnPauseButtonClick);
        }

        private void OnDisable()
        {
            _pauseButton.onClick.RemoveListener(OnPauseButtonClick);

            _pauseSubscription?.Dispose();
            _pauseSubscription = null;

            if (_activePauseMenu == null)
            {
                return;
            }

            if (_orchestrator != null)
            {
                _orchestrator.ResumeByRequest();
            }

            _uiSpawner.Release(_activePauseMenu);
            _activePauseMenu = null;
        }

        [Inject]
        public void Construct(UiSpawner uiSpawner, GameSceneState orchestrator)
        {
            _uiSpawner = uiSpawner;
            _orchestrator = orchestrator;
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
            _pauseSubscription = new PauseMenuSubscription(
                _activePauseMenu, OnPauseMenuCloseClicked, OnPauseMenuMenuClicked, OnPauseMenuClosed);
            _uiSpawner.Show(_activePauseMenu);
        }

        private void OnPauseMenuCloseClicked()
        {
            _pauseSubscription.DisableInput();

            _orchestrator.ResumeByRequest();
            _activePauseMenu.BeginClose();
        }

        private void OnPauseMenuClosed()
        {
            _pauseSubscription?.Dispose();
            _pauseSubscription = null;
            _uiSpawner.Release(_activePauseMenu);
            _activePauseMenu = null;
        }

        private void OnPauseMenuMenuClicked()
        {
            _pauseSubscription.DisableInput();

            _orchestrator.ResumeByRequest();
            _activePauseMenu.BeginClose();

            _orchestrator.ExitToMenu();
        }
    }
}
