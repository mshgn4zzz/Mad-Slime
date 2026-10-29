using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Roulette.Data;
using MadSlime.UI.Animations;
using MadSlime.UI.ResponsiveUI.Runtime;
using MadSlime.UI.Spawning;
using MadSlime.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Roulette
{
    public class RouletteView : MonoBehaviour, IShowable
    {
        private const int StackCoinThreshold = 150;
        private const int PileCoinThreshold = 300;
        private const int BagCoinThreshold = 1000;
        private const int SafeCoinThreshold = 3000;

        [SerializeField] private RouletteWheel _wheel;
        [SerializeField] private Button _spinButton;
        [SerializeField] private TMP_Text _spinPriceText;
        [SerializeField] private Button _adButton;
        [SerializeField] private TMP_Text _adButtonText;
        [SerializeField] private Sprite _secretIcon;
        [SerializeField] private Sprite _fewCoinIcon;
        [SerializeField] private Sprite _stackCoinIcon;
        [SerializeField] private Sprite _pileCoinIcon;
        [SerializeField] private Sprite _bagCoinIcon;
        [SerializeField] private Sprite _safeCoinIcon;
        [SerializeField] private RouletteWinPopup _winPopupPrefab;
        [SerializeField] private GameObject _screenRoot;
        [SerializeField] private ReactiveUIView _screenView;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private RoulettePopupLayout _layout;

        private RouletteConfig _config;
        private UniTaskCompletionSource _resultCompletion;
        private bool _isHiding;

        public event Action SpinRequested;

        public event Action AdRequested;

        public event Action CloseRequested;

        public event Action Closed;

        public event Action Disabled;

        public RouletteWheel Wheel => _wheel;

        public Sprite SecretIcon => _secretIcon;

        public RouletteWinPopup WinPopupPrefab => _winPopupPrefab;

        private void Awake()
        {
            if (_wheel == null || _spinButton == null || _spinPriceText == null || _title == null
                || _secretIcon == null || _fewCoinIcon == null || _stackCoinIcon == null || _pileCoinIcon == null
                || _bagCoinIcon == null || _safeCoinIcon == null || _winPopupPrefab == null)
            {
                throw new InvalidOperationException(
                    $"{name}: assign all roulette view references (_wheel, _spinButton, _spinPriceText, _title, "
                    + $"coin icons, _secretIcon and _winPopupPrefab).");
            }
        }

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(OnSpinClicked);

            if (_adButton != null)
            {
                _adButton.onClick.AddListener(OnAdClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseClicked);
            }

            _wheel.Completed += OnWheelCompleted;

            if (_screenView != null)
            {
                _screenView.DisableCompleted += OnHideCompleted;
            }
        }

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(OnSpinClicked);

            if (_adButton != null)
            {
                _adButton.onClick.RemoveListener(OnAdClicked);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnCloseClicked);
            }

            if (_screenView != null)
            {
                _screenView.DisableCompleted -= OnHideCompleted;
            }

            _wheel.Completed -= OnWheelCompleted;
            _isHiding = false;
            Disabled?.Invoke();
        }

        public Sprite GetCoinIcon(int amount)
        {
            ArgumentGuard.CheckPositive(amount, nameof(amount));

            Sprite icon;

            switch (amount)
            {
                case int safeAmount when safeAmount >= SafeCoinThreshold:
                    icon = _safeCoinIcon;
                    break;

                case int bagAmount when bagAmount >= BagCoinThreshold:
                    icon = _bagCoinIcon;
                    break;

                case int pileAmount when pileAmount >= PileCoinThreshold:
                    icon = _pileCoinIcon;
                    break;

                case int stackAmount when stackAmount >= StackCoinThreshold:
                    icon = _stackCoinIcon;
                    break;

                default:
                    icon = _fewCoinIcon;
                    break;
            }

            if (icon == null)
            {
                throw new InvalidOperationException($"{name}: assign the coin reward icon for amount {amount}.");
            }

            return icon;
        }

        public void Setup(RouletteConfig config)
        {
            _config = config;
            _wheel.Setup(config);
        }

        public async UniTask PresentResultAsync(int slotIndex, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UniTaskCompletionSource completed = new UniTaskCompletionSource();

            _resultCompletion = completed;

            try
            {
                _wheel.Spin(slotIndex);
                await completed.Task.AttachExternalCancellation(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Delay(
                    TimeSpan.FromSeconds(_config.WinDwellSeconds),
                    DelayType.Realtime,
                    cancellationToken: cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                if (ReferenceEquals(_resultCompletion, completed))
                {
                    _resultCompletion = null;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    _wheel.Stop();
                }
            }
        }

        public void Show()
        {
            _isHiding = false;
            gameObject.SetActive(true);

            if (_screenRoot != null)
            {
                _screenRoot.SetActive(true);
            }

            if (_screenView != null)
            {
                _screenView.Enable();
            }
        }

        public void Hide()
        {
            if (_isHiding)
            {
                return;
            }

            _isHiding = true;

            if (_screenView != null)
            {
                _screenView.Disable();
                return;
            }

            OnHideCompleted(null);
        }

        public void Display(RouletteDisplayData data)
        {
            _title.text = data.Title;

            if (_layout != null)
            {
                _layout.SetControlsVisible(data.IsSpinVisible, data.IsAdvertisementVisible);
            }

            _spinButton.gameObject.SetActive(data.IsSpinVisible);
            _spinPriceText.text = data.SpinText;
            _spinButton.interactable = data.IsSpinAvailable;

            if (_closeButton != null)
            {
                _closeButton.gameObject.SetActive(data.IsCloseVisible);
                _closeButton.interactable = data.IsClosingAllowed;
            }

            if (_adButton != null)
            {
                _adButton.gameObject.SetActive(data.IsAdvertisementVisible);
                _adButton.interactable = data.IsAdvertisementAvailable;
            }

            if (_adButtonText != null)
            {
                _adButtonText.text = data.AdText;
            }
        }

        private void OnWheelCompleted()
        {
            _resultCompletion?.TrySetResult();
        }

        private void OnHideCompleted(ReactiveUIView view)
        {
            if (_isHiding == false)
            {
                return;
            }

            _isHiding = false;
            Closed?.Invoke();

            if (_screenRoot != null)
            {
                _screenRoot.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnSpinClicked()
        {
            SpinRequested?.Invoke();
        }

        private void OnAdClicked()
        {
            AdRequested?.Invoke();
        }

        private void OnCloseClicked()
        {
            CloseRequested?.Invoke();
        }
    }
}
