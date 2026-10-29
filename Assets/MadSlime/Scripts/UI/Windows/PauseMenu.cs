using System;
using MadSlime.UI.Audio;
using MadSlime.UI.ResponsiveUI.Runtime;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Windows
{
    public class PauseMenu : BaseWindow
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private RectTransform _languageButton;
        [SerializeField] private Vector2 _languagePositionWithMenuButton;
        [SerializeField] private Vector2 _languagePositionWithoutMenuButton;
        [SerializeField] private AudioSettingsPanel _settingsPanel;
        [SerializeField] private Button _resetButton;
        [SerializeField] private ResetProgressConfirmation _resetConfirmation;
        [SerializeField] private CanvasGroup _settingsInput;

        private bool _isActionRequested;
        private bool _isResetAvailable;

        public event Action CloseButtonClicked;

        public event Action MenuButtonClicked;

        public event Action ResetConfirmed;

        protected override void Awake()
        {
            base.Awake();

            ArgumentGuard.CheckAssigned(_closeButton, name, nameof(_closeButton));

            ArgumentGuard.CheckAssigned(_menuButton, name, nameof(_menuButton));

            ArgumentGuard.CheckAssigned(_settingsPanel, name, nameof(_settingsPanel));

            ArgumentGuard.CheckAssigned(_languageButton, name, nameof(_languageButton));

            ArgumentGuard.CheckAssigned(_resetButton, name, nameof(_resetButton));
            ArgumentGuard.CheckAssigned(_resetConfirmation, name, nameof(_resetConfirmation));
            ArgumentGuard.CheckAssigned(_settingsInput, name, nameof(_settingsInput));
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _closeButton.onClick.RemoveListener(OnCloseClicked);
            _menuButton.onClick.RemoveListener(OnMenuClicked);
            _resetButton.onClick.RemoveListener(OnResetClicked);
            _resetConfirmation.Confirmed -= OnResetConfirmed;
            _resetConfirmation.Canceled -= OnResetCanceled;
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnCloseClicked);
            _menuButton.onClick.AddListener(OnMenuClicked);
            _resetButton.onClick.AddListener(OnResetClicked);
            _resetConfirmation.Confirmed += OnResetConfirmed;
            _resetConfirmation.Canceled += OnResetCanceled;
        }

        public void Initialize(bool isMenuButtonNeeded, bool isResetAvailable = false)
        {
            _isResetAvailable = isResetAvailable;
            _isActionRequested = false;
            _closeButton.interactable = true;
            _menuButton.interactable = true;
            _resetButton.gameObject.SetActive(isResetAvailable);
            _resetButton.interactable = true;
            OnResetCanceled();

            _menuButton.gameObject.SetActive(isMenuButtonNeeded);

            if (TryGetComponent(out ResponsiveCanvasLayout profiles) && profiles.IsManaging(_languageButton))
            {
                return;
            }

            if (isMenuButtonNeeded || isResetAvailable)
            {
                _languageButton.anchoredPosition = _languagePositionWithMenuButton;
            }
            else
            {
                _languageButton.anchoredPosition = _languagePositionWithoutMenuButton;
            }
        }

        protected override void OnClosing()
        {
            _closeButton.interactable = false;
            _menuButton.interactable = false;
            _resetButton.interactable = false;
        }

        private void OnResetClicked()
        {
            if (_isResetAvailable == false || _isActionRequested || IsClosing())
            {
                return;
            }

            _settingsInput.interactable = false;
            _settingsInput.blocksRaycasts = false;
            _resetConfirmation.Show();
        }

        private void OnResetCanceled()
        {
            _resetConfirmation.Hide();
            _settingsInput.interactable = true;
            _settingsInput.blocksRaycasts = true;

            if (_resetButton.gameObject.activeInHierarchy)
            {
                _resetButton.Select();
            }
        }

        private void OnResetConfirmed()
        {
            if (_isResetAvailable == false || _isActionRequested || IsClosing())
            {
                return;
            }

            _isActionRequested = true;
            _resetConfirmation.Hide();
            ResetConfirmed?.Invoke();
        }

        private void OnCloseClicked()
        {
            if (_isActionRequested || IsClosing())
            {
                return;
            }

            _isActionRequested = true;

            Action closeButtonClicked = CloseButtonClicked;

            closeButtonClicked?.Invoke();
        }

        private void OnMenuClicked()
        {
            if (_isActionRequested || IsClosing())
            {
                return;
            }

            _isActionRequested = true;

            Action menuButtonClicked = MenuButtonClicked;

            menuButtonClicked?.Invoke();
        }
    }
}
