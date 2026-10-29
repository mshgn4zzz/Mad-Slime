using System;
using MadSlime.Economy.Data;
using MadSlime.Economy.Logic;
using MadSlime.Saves.Interfaces;
using MadSlime.StateMachine.Logic;
using MadSlime.UI.Fill.StateMachine.States;

namespace MadSlime.UI.Fill.StateMachine
{
    public class FillUiStateMachine : BaseStateMachine<IFillUiState>, IDisposable
    {
        private readonly ActiveFillUiState _active = new ActiveFillUiState();
        private readonly PausedFillUiState _paused = new PausedFillUiState();
        private readonly PresentingResultFillUiState _presenting = new PresentingResultFillUiState();
        private readonly WinFillUiState _win = new WinFillUiState();
        private readonly FailFillUiState _fail = new FailFillUiState();
        private readonly DoubleRewardAdState _doubleAd = new DoubleRewardAdState();
        private readonly DoubleRewardSaveState _saving;
        private readonly DoubleRewardSaveFailedState _saveError = new DoubleRewardSaveFailedState();
        private readonly RewardedWinFillUiState _rewarded = new RewardedWinFillUiState();
        private readonly RescueAdState _rescue = new RescueAdState();
        private readonly ClosingFillUiState _closing = new ClosingFillUiState();
        private readonly DoubleRewardModel _rewards;
        private FillUiResult _result;
        private DoubleRewardReceipt _receipt;
        private FillUiAction _action;
        private int _lastFailReward;

        public FillUiStateMachine(DoubleRewardModel rewards, ISaveConfirmation confirmation)
        {
            _rewards = rewards;
            _saving = new DoubleRewardSaveState(confirmation);
            RegisterState(_active);
            RegisterState(_paused);
            RegisterState(_presenting);
            RegisterState(_win);
            RegisterState(_fail);
            RegisterState(_doubleAd);
            RegisterState(_saving);
            RegisterState(_saveError);
            RegisterState(_rewarded);
            RegisterState(_rescue);
            RegisterState(_closing);
            Subscribe();
        }

        public event Action<FillUiResult> ResultPresentationRequested;
        public event Action PauseRequested;
        public event Action WindowCloseRequested;
        public event Action<Action<bool>> DoubleRewardAdRequested;
        public event Action<Action<bool>> RescueAdRequested;
        public event Action<int> RewardChanged;
        public event Action<FillUiAction> NavigationRequested;
        public event Action InteractionChanged;
        public event Action AccelerateRequested;
        public event Action<Exception> OperationFailed;

        public void Start()
        {
            if (CurrentState == null)
            {
                SwitchState<ActiveFillUiState>();
            }
        }

        public bool IsPauseAvailable()
        {
            return CurrentState == _active;
        }

        public bool IsDoubleRewardAvailable()
        {
            return CurrentState == _win || CurrentState == _saveError;
        }

        public bool IsRescueAvailable()
        {
            return CurrentState == _fail && _result.IsRescueAvailable;
        }

        public bool IsSaveFailed()
        {
            return CurrentState == _saveError;
        }

        public void PresentWin(int reward)
        {
            Present(new FillUiResult(true, reward, _lastFailReward, false));
        }

        public void PresentFail(int reward, bool isRescueAvailable)
        {
            _lastFailReward = reward;
            Present(new FillUiResult(false, reward, 0, isRescueAvailable));
        }

        public void PreparationFailed()
        {
            PresentFail(_lastFailReward, false);
        }

        public void NotifyResultShown()
        {
            if (CurrentState != _presenting)
            {
                return;
            }

            if (_result.IsWin)
            {
                SwitchState<WinFillUiState>();
            }
            else
            {
                SwitchState<FailFillUiState>();
            }
        }

        public void RequestPause()
        {
            if (IsPauseAvailable())
            {
                SwitchState<PausedFillUiState>();
            }
        }

        public void RequestAccelerate()
        {
            if (CurrentState == _active)
            {
                AccelerateRequested?.Invoke();
            }
        }

        public void RequestDoubleReward()
        {
            if (CurrentState == _win)
            {
                SwitchState<DoubleRewardAdState>();
            }
            else if (CurrentState == _saveError)
            {
                SwitchState<DoubleRewardSaveState>();
            }

            OnChanged();
        }

        public void RequestRescue()
        {
            if (IsRescueAvailable())
            {
                SwitchState<RescueAdState>();
            }
        }

        public void RequestAction(FillUiAction action)
        {
            if (CurrentState == null || CurrentState == _active || CurrentState == _presenting
                || CurrentState == _closing)
            {
                return;
            }

            if (action == FillUiAction.Rescue && CurrentState != _rescue)
            {
                return;
            }

            if (action == FillUiAction.Continue && CurrentState != _paused)
            {
                return;
            }

            _action = action;
            SwitchState<ClosingFillUiState>();
        }

        public void NotifyWindowClosed()
        {
            if (CurrentState != _closing)
            {
                return;
            }

            FillUiAction action = _action;

            if (action == FillUiAction.Continue || action == FillUiAction.Rescue)
            {
                SwitchState<ActiveFillUiState>();
            }
            else
            {
                Stop();
            }

            if (action != FillUiAction.Continue)
            {
                NavigationRequested?.Invoke(action);
            }
        }

        public override void Stop()
        {
            base.Stop();
            _result = null;
            _receipt = null;
        }

        public void Dispose()
        {
            Stop();
            Unsubscribe();
        }

        private void Present(FillUiResult result)
        {
            if (CurrentState == null)
            {
                return;
            }

            base.Stop();
            _result = result;
            _receipt = null;
            SwitchState<PresentingResultFillUiState>();
        }

        private void Subscribe()
        {
            _active.Entered += OnChanged;
            _paused.Entered += OnPause;
            _presenting.Entered += OnPresent;
            _win.Entered += OnChanged;
            _fail.Entered += OnChanged;
            _doubleAd.Requested += OnDoubleAd;
            _doubleAd.Granted += OnDoubleGranted;
            _doubleAd.Rejected += OnDoubleRejected;
            _saving.Confirmed += OnSaved;
            _saving.Failed += OnSaveFailed;
            _saveError.Entered += OnChanged;
            _rewarded.Entered += OnRewarded;
            _rescue.Requested += OnRescueAd;
            _rescue.Granted += OnRescueGranted;
            _rescue.Rejected += OnRescueRejected;
            _closing.Entered += OnClosing;
        }

        private void Unsubscribe()
        {
            _active.Entered -= OnChanged;
            _paused.Entered -= OnPause;
            _presenting.Entered -= OnPresent;
            _win.Entered -= OnChanged;
            _fail.Entered -= OnChanged;
            _doubleAd.Requested -= OnDoubleAd;
            _doubleAd.Granted -= OnDoubleGranted;
            _doubleAd.Rejected -= OnDoubleRejected;
            _saving.Confirmed -= OnSaved;
            _saving.Failed -= OnSaveFailed;
            _saveError.Entered -= OnChanged;
            _rewarded.Entered -= OnRewarded;
            _rescue.Requested -= OnRescueAd;
            _rescue.Granted -= OnRescueGranted;
            _rescue.Rejected -= OnRescueRejected;
            _closing.Entered -= OnClosing;
        }

        private void OnChanged()
        {
            InteractionChanged?.Invoke();
        }

        private void OnPause()
        {
            OnChanged();
            PauseRequested?.Invoke();
        }

        private void OnPresent()
        {
            OnChanged();
            ResultPresentationRequested?.Invoke(_result);
        }

        private void OnDoubleAd(Action<bool> complete)
        {
            OnChanged();
            DoubleRewardAdRequested?.Invoke(complete);
        }

        private void OnRescueAd(Action<bool> complete)
        {
            OnChanged();
            RescueAdRequested?.Invoke(complete);
        }

        private void OnDoubleGranted()
        {
            if (CurrentState != _doubleAd)
            {
                return;
            }

            try
            {
                _receipt = _rewards.Grant(_result.Reward);
            }
            catch (Exception exception)
            {
                SwitchState<WinFillUiState>();
                OperationFailed?.Invoke(exception);
                return;
            }

            SwitchState<DoubleRewardSaveState>();
            OnChanged();
        }

        private void OnDoubleRejected()
        {
            if (CurrentState == _doubleAd)
            {
                SwitchState<WinFillUiState>();
            }
        }

        private void OnSaved()
        {
            if (CurrentState == _saving)
            {
                SwitchState<RewardedWinFillUiState>();
            }
        }

        private void OnSaveFailed(Exception exception)
        {
            if (CurrentState == _saving)
            {
                SwitchState<DoubleRewardSaveFailedState>();
            }
        }

        private void OnRewarded()
        {
            OnChanged();
            RewardChanged?.Invoke(_receipt.TotalReward);
        }

        private void OnRescueGranted()
        {
            if (CurrentState == _rescue)
            {
                RequestAction(FillUiAction.Rescue);
            }
        }

        private void OnRescueRejected()
        {
            if (CurrentState == _rescue)
            {
                SwitchState<FailFillUiState>();
            }
        }

        private void OnClosing()
        {
            OnChanged();
            WindowCloseRequested?.Invoke();
        }
    }
}
