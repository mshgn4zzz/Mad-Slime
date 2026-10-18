using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ResetProgressConfirmation : MonoBehaviour
    {
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        public event Action Confirmed;
        public event Action Canceled;

        private void Awake()
        {
            if (_confirmButton == null || _cancelButton == null)
            {
                throw new InvalidOperationException($"{name}: assign both confirmation buttons.");
            }
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
