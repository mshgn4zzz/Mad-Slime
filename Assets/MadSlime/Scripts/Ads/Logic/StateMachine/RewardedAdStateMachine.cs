using System;
using MadSlime.Ads.Interfaces;
using MadSlime.Ads.Logic.StateMachine.States;
using MadSlime.StateMachine.Logic;

namespace MadSlime.Ads.Logic.StateMachine
{
    public class RewardedAdStateMachine : BaseStateMachine<IRewardedAdState>, IDisposable
    {
        private readonly IAdsService _service;
        private readonly AvailableAdState _available = new AvailableAdState();
        private readonly RequestedAdState _requested = new RequestedAdState();
        private readonly ShowingAdState _showing = new ShowingAdState();
        private readonly RewardPendingCloseAdState _reward = new RewardPendingCloseAdState();
        private RewardedAdRequest _request;

        public RewardedAdStateMachine(IAdsService service)
        {
            _service = service;
            RegisterState(_available);
            RegisterState(_requested);
            RegisterState(_showing);
            RegisterState(_reward);
            Subscribe();
            SwitchState<AvailableAdState>();
        }

        public bool IsAvailable()
        {
            return CurrentState == _available && _service.IsAdShowing == false;
        }

        public bool TrySubmit(RewardedAdRequest request)
        {
            if (IsAvailable() == false)
            {
                request.Reject();
                return false;
            }

            _request = request;
            SwitchState<RequestedAdState>();

            try
            {
                _service.ShowRewarded(request.RewardId);
            }
            catch (Exception)
            {
                if (ReferenceEquals(_request, request))
                {
                    OnError();
                }
                else
                {
                    throw;
                }
            }

            return true;
        }

        public void CancelConsumer(RewardedAdRequest request)
        {
            request?.Detach();
        }

        public bool TryShowInterstitial()
        {
            if (IsAvailable() == false)
            {
                return false;
            }

            _service.ShowInterstitial();
            return true;
        }

        public void Dispose()
        {
            Unsubscribe();
            _request?.Detach();
            _request = null;
            Stop();
        }

        private void Subscribe()
        {
            _service.RewardedOpened += OnOpened;
            _service.RewardReceived += OnReward;
            _service.RewardedClosed += OnClosed;
            _service.RewardedFailed += OnError;
        }

        private void Unsubscribe()
        {
            _service.RewardedOpened -= OnOpened;
            _service.RewardReceived -= OnReward;
            _service.RewardedClosed -= OnClosed;
            _service.RewardedFailed -= OnError;
        }

        private void OnOpened()
        {
            if (CurrentState == _requested)
            {
                SwitchState<ShowingAdState>();
            }
        }

        private void OnReward(string rewardId)
        {
            if (_request != null && _request.RewardId == rewardId
                && (CurrentState == _requested
                || CurrentState == _showing))
            {
                SwitchState<RewardPendingCloseAdState>();
            }
        }

        private void OnClosed()
        {
            if (_request == null)
            {
                return;
            }

            bool isGranted = CurrentState == _reward;
            RewardedAdRequest completed = _request;

            _request = null;
            SwitchState<AvailableAdState>();

            if (isGranted)
            {
                completed.Grant();
            }
            else
            {
                completed.CompleteWithoutReward();
            }
        }

        private void OnError()
        {
            if (_request == null)
            {
                return;
            }

            RewardedAdRequest completed = _request;

            _request = null;
            SwitchState<AvailableAdState>();
            completed.Fail();
        }
    }
}
