using System;
using MadSlime.Ads.Logic.StateMachine;
using MadSlime.Game.Data;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Ads.Logic
{
    public class AdScheduler : MonoBehaviour
    {
        [SerializeField]
        private YandexConfig _config;
        private RewardedAdStateMachine _machine;
        private RewardedAdRequest _request;

        public string RouletteRewardId => _config.RouletteRewardId;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));
            ArgumentGuard.CheckAssigned(_machine, name, nameof(_machine));
        }

        private void OnDisable()
        {
            _machine?.CancelConsumer(_request);
            _request = null;
        }

        [Inject]
        public void Construct(RewardedAdStateMachine machine)
        {
            _machine = machine;
        }

        public bool TryShowInterstitial()
        {
            return _machine.TryShowInterstitial();
        }

        public void ShowDoubleReward(Action onGranted, Action onNotGranted)
        {
            ShowRewarded(_config.DoubleRewardId, onGranted, onNotGranted, onNotGranted, onNotGranted);
        }

        public void ShowRewarded(
            string rewardId,
            Action onGranted,
            Action onCompletedWithoutReward,
            Action onRejected,
            Action onError)
        {
            RewardedAdRequest request = new RewardedAdRequest(
                rewardId,
                onGranted,
                onCompletedWithoutReward,
                onRejected,
                onError);

            if (_machine.IsAvailable() == false)
            {
                request.Reject();
                return;
            }

            _request = request;
            _machine.TrySubmit(request);
        }
    }
}
