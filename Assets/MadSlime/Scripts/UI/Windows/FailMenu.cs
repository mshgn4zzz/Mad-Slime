using System;
using MadSlime.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Windows
{
    public class FailMenu : BaseWindow
    {
        [SerializeField] private TMP_Text _moneyCount;
        [SerializeField] private Button _rescueButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        private bool _isNavigationRequested;

        public event Action RescueRequested;

        public event Action RestartRequested;

        public event Action MenuRequested;

        protected override void Awake()
        {
            base.Awake();

            ArgumentGuard.CheckAssigned(_moneyCount, name, nameof(_moneyCount));

            ArgumentGuard.CheckAssigned(_rescueButton, name, nameof(_rescueButton));

            ArgumentGuard.CheckAssigned(_restartButton, name, nameof(_restartButton));

            ArgumentGuard.CheckAssigned(_menuButton, name, nameof(_menuButton));
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _rescueButton.onClick.RemoveListener(OnRescueClicked);
            _restartButton.onClick.RemoveListener(OnRestartClicked);
            _menuButton.onClick.RemoveListener(OnMenuClicked);
        }

        private void OnEnable()
        {
            _rescueButton.onClick.AddListener(OnRescueClicked);
            _restartButton.onClick.AddListener(OnRestartClicked);
            _menuButton.onClick.AddListener(OnMenuClicked);
        }

        public void Initialize(int moneyCount, bool isRescueAvailable)
        {
            _isNavigationRequested = false;
            _restartButton.interactable = true;
            _menuButton.interactable = true;
            _moneyCount.text = moneyCount.ToString();

            _rescueButton.interactable = isRescueAvailable;
            _rescueButton.gameObject.SetActive(isRescueAvailable);
        }

        public void SetRescueAvailable(bool isAvailable)
        {
            _rescueButton.interactable = isAvailable;
        }

        protected override void OnClosing()
        {
            _rescueButton.interactable = false;
            _restartButton.interactable = false;
            _menuButton.interactable = false;
        }

        private void OnRescueClicked()
        {
            if (_isNavigationRequested || IsClosing())
            {
                return;
            }

            Action rescueRequested = RescueRequested;

            rescueRequested?.Invoke();
        }

        private void OnRestartClicked()
        {
            if (_isNavigationRequested || IsClosing())
            {
                return;
            }

            _isNavigationRequested = true;

            Action restartRequested = RestartRequested;

            restartRequested?.Invoke();
        }

        private void OnMenuClicked()
        {
            if (_isNavigationRequested || IsClosing())
            {
                return;
            }

            _isNavigationRequested = true;

            Action menuRequested = MenuRequested;

            menuRequested?.Invoke();
        }
    }
}
