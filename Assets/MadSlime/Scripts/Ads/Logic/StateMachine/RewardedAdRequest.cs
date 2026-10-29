using System;
using MadSlime.Validation;

namespace MadSlime.Ads.Logic.StateMachine
{
    public class RewardedAdRequest
    {
        private Action _granted;
        private Action _withoutReward;
        private Action _rejected;
        private Action _error;

        public RewardedAdRequest(
            string rewardId,
            Action granted,
            Action withoutReward,
            Action rejected,
            Action error)
        {
            ArgumentGuard.CheckNotEmpty(rewardId, nameof(rewardId));
            ArgumentGuard.CheckNotNull(granted, nameof(granted));
            RewardId = rewardId;
            _granted = granted;
            _withoutReward = withoutReward;
            _rejected = rejected;
            _error = error;
        }

        public string RewardId { get; }

        public void Grant()
        {
            _granted?.Invoke();
        }

        public void CompleteWithoutReward()
        {
            _withoutReward?.Invoke();
        }

        public void Reject()
        {
            _rejected?.Invoke();
        }

        public void Fail()
        {
            _error?.Invoke();
        }

        public void Detach()
        {
            _granted = null;
            _withoutReward = null;
            _rejected = null;
            _error = null;
        }
    }
}
