using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.Windows
{
    using Localization = global::MadSlime.Localization.Logic.Localization;

    public class WinMenu : BaseWindow
    {
        [VContainer.Inject] private Localization _localization;

        [SerializeField]
        private TMP_Text _moneyCount;
        [SerializeField]
        private Button _nextLevelButton;
        [SerializeField]
        private Button _doubleRewardButton;
        [SerializeField]
        private Button _menuButton;

        [SerializeField, Min(0.01f)]
        private float _countUpDuration = 0.8f;

        private int _moneyCountShown;
        private string _doubleRewardLabel;
        private CancellationTokenSource _countUpCancellation;
        private bool _isNavigationRequested;

        public event Action NextLevelRequested;

        public event Action DoubleRewardRequested;

        public event Action MenuRequested;

        protected override void Awake()
        {
            base.Awake();

            ArgumentGuard.CheckAssigned(_moneyCount, name, nameof(_moneyCount));

            ArgumentGuard.CheckAssigned(_nextLevelButton, name, nameof(_nextLevelButton));

            ArgumentGuard.CheckAssigned(_doubleRewardButton, name, nameof(_doubleRewardButton));

            ArgumentGuard.CheckAssigned(_menuButton, name, nameof(_menuButton));
        }

        protected override void OnDisable()
        {
            CancelCountUp();
            base.OnDisable();
            _nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
            _doubleRewardButton.onClick.RemoveListener(OnDoubleRewardClicked);
            _menuButton.onClick.RemoveListener(OnMenuClicked);
        }

        private void OnEnable()
        {
            _nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            _doubleRewardButton.onClick.AddListener(OnDoubleRewardClicked);
            _menuButton.onClick.AddListener(OnMenuClicked);
        }

        public void Initialize(int moneyCount, int previousMoneyCount, bool isCanDoubleReward)
        {
            CancelCountUp();
            _isNavigationRequested = false;
            _nextLevelButton.interactable = true;
            _menuButton.interactable = true;
            _doubleRewardButton.interactable = isCanDoubleReward;
            _moneyCountShown = previousMoneyCount;
            _moneyCount.text = previousMoneyCount.ToString();

            _doubleRewardButton.gameObject.SetActive(isCanDoubleReward);
        }

        public void PlayRewardCountUp(int targetAmount)
        {
            CancelCountUp();

            if (IsClosing())
            {
                return;
            }

            if (targetAmount <= _moneyCountShown)
            {
                _moneyCountShown = targetAmount;
                _moneyCount.text = targetAmount.ToString();
                return;
            }

            _countUpCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy());
            CountUpMoneyAsync(_moneyCountShown, targetAmount, _countUpCancellation.Token).Forget();
        }

        public void SetDoubleRewardAvailable(bool isAvailable)
        {
            _doubleRewardButton.interactable = isAvailable;
        }

        public void SetDoubleRewardRetry(bool isRetry)
        {
            TMP_Text label = _doubleRewardButton.GetComponentInChildren<TMP_Text>(true);

            if (label == null)
            {
                return;
            }

            if (_doubleRewardLabel == null)
            {
                _doubleRewardLabel = label.text;
            }

            if (isRetry)
            {
                string error = _localization.Get("roulette_save_error");
                string retryPrompt = _localization.Get("leaderboard_retry");

                label.text = error + "\n" + retryPrompt;
            }
            else
            {
                label.text = _doubleRewardLabel;
            }
        }

        private async UniTaskVoid CountUpMoneyAsync(
            int fromAmount,
            int toAmount,
            CancellationToken cancellationToken)
        {
            float elapsedTime = 0f;

            try
            {
                while (elapsedTime < _countUpDuration)
                {
                    elapsedTime += Time.unscaledDeltaTime;

                    float progress = Mathf.Clamp01(elapsedTime / _countUpDuration);
                    int shownAmount = Mathf.RoundToInt(Mathf.Lerp(fromAmount, toAmount, progress));

                    _moneyCountShown = shownAmount;
                    _moneyCount.text = shownAmount.ToString();
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _moneyCountShown = toAmount;
            _moneyCount.text = toAmount.ToString();
        }

        private void CancelCountUp()
        {
            if (_countUpCancellation == null)
            {
                return;
            }

            _countUpCancellation.Cancel();
            _countUpCancellation.Dispose();
            _countUpCancellation = null;
        }

        protected override void OnClosing()
        {
            CancelCountUp();
            _nextLevelButton.interactable = false;
            _doubleRewardButton.interactable = false;
            _menuButton.interactable = false;
        }

        private void OnNextLevelClicked()
        {
            if (_isNavigationRequested || IsClosing())
            {
                return;
            }

            _isNavigationRequested = true;

            Action nextLevelRequested = NextLevelRequested;

            nextLevelRequested?.Invoke();
        }

        private void OnDoubleRewardClicked()
        {
            if (_isNavigationRequested || IsClosing())
            {
                return;
            }

            Action doubleRewardRequested = DoubleRewardRequested;

            doubleRewardRequested?.Invoke();
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
