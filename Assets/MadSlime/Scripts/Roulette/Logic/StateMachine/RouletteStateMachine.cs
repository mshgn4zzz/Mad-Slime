using System;
using MadSlime.Roulette.Logic.StateMachine.States;
using MadSlime.Saves.Interfaces;
using MadSlime.StateMachine.Logic;
using MadSlime.Validation;

namespace MadSlime.Roulette.Logic.StateMachine
{
    public class RouletteStateMachine : BaseStateMachine<IRouletteState>, IDisposable
    {
        private readonly RouletteReadyState _ready;
        private readonly RouletteWaitingForAdState _advertising;
        private readonly RouletteCommittingSpinState _committing;
        private readonly RouletteConfirmingSaveState _confirming;
        private readonly RouletteSaveFailedState _saveFailed;
        private readonly RouletteResultState _result;
        private readonly RouletteSessionData _session = new RouletteSessionData();
        private readonly bool _isClosingAfterPrize;

        public RouletteStateMachine(RouletteModel model, ISaveConfirmation confirmation, bool isClosingAfterPrize)
        {
            ArgumentGuard.CheckNotNull(model, nameof(model));
            ArgumentGuard.CheckNotNull(confirmation, nameof(confirmation));
            Model = model;
            _isClosingAfterPrize = isClosingAfterPrize;
            _ready = new RouletteReadyState(model, _session);
            _advertising = new RouletteWaitingForAdState();
            _committing = new RouletteCommittingSpinState(model, _session);
            _confirming = new RouletteConfirmingSaveState(confirmation);
            _saveFailed = new RouletteSaveFailedState();
            _result = new RouletteResultState(_session);
            RegisterState(_ready);
            RegisterState(_advertising);
            RegisterState(_committing);
            RegisterState(_confirming);
            RegisterState(_saveFailed);
            RegisterState(_result);
            Subscribe();
        }

        public event Action Prepared;
        public event Action Changed;
        public event Action<Action<bool>> AdvertisementRequested;
        public event Action<RouletteOutcome> ResultPrepared;
        public event Action CloseRequested;
        public event Action<Exception> Failed;

        public RouletteModel Model { get; }

        public void Start()
        {
            if (IsRunning() == false)
            {
                SwitchState<RouletteReadyState>();
            }
        }

        public bool IsRunning()
        {
            return CurrentState != null;
        }

        public bool IsReady()
        {
            return CurrentState == _ready;
        }

        public bool IsSaveFailed()
        {
            return CurrentState == _saveFailed;
        }

        public bool IsSpinAvailable()
        {
            return IsSaveFailed() || (IsReady() && Model.IsSpinAvailable(false));
        }

        public bool IsAdvertisementAvailable()
        {
            return IsReady() && Model.IsAdvertisingAvailable && Model.IsSpinAvailable(true);
        }

        public bool IsClosingAllowed()
        {
            return Model.IsAdvertisingAvailable && (IsReady() || IsSaveFailed());
        }

        public bool TrySpin()
        {
            if (IsSaveFailed())
            {
                SwitchState<RouletteConfirmingSaveState>();
                OnChanged();
                return true;
            }

            if (IsReady() == false || Model.IsSpinAvailable(false) == false)
            {
                return false;
            }

            _session.IsAdvertisementGranted = false;
            SwitchState<RouletteCommittingSpinState>();
            OnChanged();
            return true;
        }

        public bool TryAdvertisement()
        {
            if (IsAdvertisementAvailable() == false)
            {
                return false;
            }

            SwitchState<RouletteWaitingForAdState>();
            OnChanged();
            return true;
        }

        public void RequestClose()
        {
            if (IsClosingAllowed())
            {
                CloseRequested?.Invoke();
            }
        }

        public void CompletePresentation()
        {
            if (CurrentState != _result)
            {
                return;
            }

            if (_isClosingAfterPrize)
            {
                CloseRequested?.Invoke();
            }
            else
            {
                SwitchState<RouletteReadyState>();
            }
        }

        public override void Stop()
        {
            base.Stop();
            _session.Clear();
        }

        public void Dispose()
        {
            Stop();
            Unsubscribe();
        }

        private void Subscribe()
        {
            _ready.Entered += OnReady;
            _advertising.Requested += OnAdvertisementRequested;
            _advertising.Granted += OnAdGranted;
            _advertising.Rejected += OnAdRejected;
            _committing.Committed += OnCommitted;
            _committing.Rejected += OnCommitRejected;
            _committing.Failed += OnCommitFailed;
            _confirming.Confirmed += OnConfirmed;
            _confirming.Failed += OnSaveFailed;
            _saveFailed.Entered += OnChanged;
            _result.ResultPrepared += OnResultReady;
        }

        private void Unsubscribe()
        {
            _ready.Entered -= OnReady;
            _advertising.Requested -= OnAdvertisementRequested;
            _advertising.Granted -= OnAdGranted;
            _advertising.Rejected -= OnAdRejected;
            _committing.Committed -= OnCommitted;
            _committing.Rejected -= OnCommitRejected;
            _committing.Failed -= OnCommitFailed;
            _confirming.Confirmed -= OnConfirmed;
            _confirming.Failed -= OnSaveFailed;
            _saveFailed.Entered -= OnChanged;
            _result.ResultPrepared -= OnResultReady;
        }

        private void OnReady()
        {
            Prepared?.Invoke();
            OnChanged();
        }

        private void OnChanged()
        {
            Changed?.Invoke();
        }

        private void OnAdvertisementRequested(Action<bool> complete)
        {
            OnChanged();
            AdvertisementRequested?.Invoke(complete);
        }

        private void OnAdGranted()
        {
            if (CurrentState == _advertising)
            {
                _session.IsAdvertisementGranted = true;
                SwitchState<RouletteCommittingSpinState>();
                OnChanged();
            }
        }

        private void OnAdRejected()
        {
            if (CurrentState == _advertising)
            {
                SwitchState<RouletteReadyState>();
            }
        }

        private void OnCommitted()
        {
            if (CurrentState == _committing)
            {
                SwitchState<RouletteConfirmingSaveState>();
            }
        }

        private void OnCommitRejected()
        {
            if (CurrentState == _committing)
            {
                SwitchState<RouletteReadyState>();
            }
        }

        private void OnCommitFailed(Exception exception)
        {
            if (CurrentState == _committing)
            {
                Failed?.Invoke(exception);
                CloseRequested?.Invoke();
            }
        }

        private void OnConfirmed()
        {
            if (CurrentState == _confirming)
            {
                SwitchState<RouletteResultState>();
            }
        }

        private void OnSaveFailed(Exception exception)
        {
            if (CurrentState == _confirming)
            {
                SwitchState<RouletteSaveFailedState>();
            }
        }

        private void OnResultReady(RouletteOutcome outcome)
        {
            OnChanged();
            ResultPrepared?.Invoke(outcome);
        }
    }
}
