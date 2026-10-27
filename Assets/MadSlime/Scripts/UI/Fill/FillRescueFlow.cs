using System;
using Ads;
using Game;

namespace UI.Fill
{
    public class FillRescueFlow
    {
        private AdScheduler _adScheduler;
        private YandexConfig _yandexConfig;
        private Func<bool> _canStart;
        private Action _resolved;
        private Action _notGranted;
        private bool _isRequestRunning;
        private bool _isResolved;

        public FillRescueFlow(
            AdScheduler adScheduler,
            YandexConfig yandexConfig,
            Func<bool> canStart,
            Action resolved,
            Action notGranted)
        {
            if (adScheduler == null)
            {
                throw new ArgumentNullException(nameof(adScheduler));
            }

            if (yandexConfig == null)
            {
                throw new ArgumentNullException(nameof(yandexConfig));
            }

            if (canStart == null)
            {
                throw new ArgumentNullException(nameof(canStart));
            }

            if (resolved == null)
            {
                throw new ArgumentNullException(nameof(resolved));
            }

            _adScheduler = adScheduler;
            _yandexConfig = yandexConfig;
            _canStart = canStart;
            _resolved = resolved;
            ArgumentGuard.CheckNotNull(notGranted, nameof(notGranted));
            _notGranted = notGranted;
        }

        public void Reset()
        {
            _isRequestRunning = false;
            _isResolved = false;
        }

        public void StopRequest()
        {
            _isRequestRunning = false;
        }

        public void Begin()
        {
            if (_canStart() == false || _isRequestRunning == true || _isResolved == true)
            {
                return;
            }

            _isRequestRunning = true;
            _adScheduler.ShowRewarded(
                _yandexConfig.FillRescueRewardId,
                Resolve,
                OnAdCompletedWithoutReward,
                OnAdCompletedWithoutReward,
                OnAdCompletedWithoutReward);
        }

        private void Resolve()
        {
            if (_canStart() == false || _isResolved == true || _isRequestRunning == false)
            {
                return;
            }

            _isRequestRunning = false;
            _isResolved = true;
            _resolved();
        }

        private void OnAdCompletedWithoutReward()
        {
            if (_canStart() == true)
            {
                _isRequestRunning = false;
                _notGranted();
            }
        }
    }
}
