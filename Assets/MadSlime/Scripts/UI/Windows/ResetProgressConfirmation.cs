using System;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Windows
{
    public class ResetProgressConfirmation : MonoBehaviour
    {
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        public event Action Confirmed;

        public event Action Canceled;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_confirmButton, name, nameof(_confirmButton));
            ArgumentGuard.CheckAssigned(_cancelButton, name, nameof(_cancelButton));
        }

        private void OnEnable()
        {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            _cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _cancelButton.Select();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnConfirmClicked()
        {
            Confirmed?.Invoke();
        }

        private void OnCancelClicked()
        {
            Canceled?.Invoke();
        }
    }
}
