using Game;
using System;
using UnityEngine;
using VContainer;
namespace Ads
{
    public class AdScheduler : MonoBehaviour
    {
        [SerializeField] private YandexConfig _config;

        private IAdsService _adsService;
        private Action _pendingGrantedAction;
        private Action _pendingCompletedWithoutRewardAction;
        private Action _pendingRejectedAction;
        private Action _pendingErrorAction;
        private bool _rewardedReceived;
        private bool _isRewardedRequestRunning;

        [Inject]
        public void Construct(IAdsService adsService)
        {
            _adsService = adsService;
        }

        private void Awake()
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: YandexConfig is not assigned. Create a YandexConfig asset and "
                        + $"drag it into the _config field.");
            }

            if (_adsService == null)
            {
                throw new InvalidOperationException(
                    $"{name}: IAdsService was not injected. Check that ProjectLifetimeScope "
                        + $"registers the YG2 ads adapter.");
            }
        }

        private void OnEnable()
        {
            _adsService.RewardedOpened += OnRewardedOpened;
            _adsService.RewardReceived += OnRewardReceived;
            _adsService.RewardedClosed += OnRewardedClosed;
            _adsService.RewardedError += OnRewardedError;
        }

        private void OnDisable()
        {
            _adsService.RewardedOpened -= OnRewardedOpened;
            _adsService.RewardReceived -= OnRewardReceived;
            _adsService.RewardedClosed -= OnRewardedClosed;
            _adsService.RewardedError -= OnRewardedError;
        }

        public bool TryShowInterstitial()
        {
            if (_adsService.IsAdShowing == true)
            {
                return false;
            }

            _adsService.ShowInterstitial();
            return true;
        }

        public string RouletteRewardId => _config.RouletteRewardId;

        public void ShowDoubleReward(Action onGranted, Action onNotGranted)
        {
            ShowRewarded(_config.DoubleRewardId, onGranted, onNotGranted, onNotGranted, onNotGranted);
        }

        public void ShowRewarded(string rewardId, Action onGranted, Action onCompletedWithoutReward,
            Action onRejected, Action onError)
        {
            if (onGranted == null)
            {
                throw new ArgumentNullException(nameof(onGranted));
            }

            if (_isRewardedRequestRunning == true || _adsService.IsAdShowing == true)
            {
                onRejected?.Invoke();
                return;
            }

            _rewardedReceived = false;
            _isRewardedRequestRunning = true;
            _pendingGrantedAction = onGranted;
            _pendingCompletedWithoutRewardAction = onCompletedWithoutReward;
            _pendingRejectedAction = onRejected;
            _pendingErrorAction = onError;
            _adsService.ShowRewarded(rewardId);
        }

        private void OnRewardedOpened()
        {
            _rewardedReceived = false;
        }

        private void OnRewardReceived(string rewardId)
        {
            _rewardedReceived = true;
        }

        private void OnRewardedClosed()
        {
            if (_isRewardedRequestRunning == false)
            {
                return;
            }

            _isRewardedRequestRunning = false;
            Action granted = _pendingGrantedAction;
            Action completedWithoutReward = _pendingCompletedWithoutRewardAction;
            ClearPendingActions();

            if (_rewardedReceived == true)
            {
                granted?.Invoke();
                return;
            }

            completedWithoutReward?.Invoke();
        }

        private void OnRewardedError()
        {
            if (_isRewardedRequestRunning == false)
            {
                return;
            }

            _isRewardedRequestRunning = false;
            Action error = _pendingErrorAction;
            ClearPendingActions();

            error?.Invoke();
        }

        private void ClearPendingActions()
        {
            _pendingGrantedAction = null;
            _pendingCompletedWithoutRewardAction = null;
            _pendingRejectedAction = null;
            _pendingErrorAction = null;
        }
    }
}
