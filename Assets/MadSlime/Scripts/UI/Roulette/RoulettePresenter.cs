using Ads;
using Audio;
using Cysharp.Threading.Tasks;
using Roulette;
using Saves;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace UI.Roulette
{
    using Localization = global::Localization.Localization;

    public class RoulettePresenter : MonoBehaviour
    {
        private const float ButtonRefreshInterval = 1f;

        private enum RouletteState
        {
            Hidden,
            Idle,
            WaitingForAd,
            ConfirmingSave,
            Spinning,
            SaveFailed
        }

        [SerializeField] private RouletteView _view;

        private RouletteModel _model;
        private ISaveConfirmation _saveConfirmation;
        private AdScheduler _adScheduler;
        private RouletteSoundFeedback _sound;
        private IUISoundPlayer _soundPlayer;
        private RouletteOutcome _outcome;
        private CancellationTokenSource _presentationCancellation;
        private RouletteState _state = RouletteState.Hidden;
        private bool _isSubscribed;
        private Action<RoulettePrize> _onComplete;
        private float _nextRefreshTime;

        public event Action CloseRequested;

        public event Action Closed;

        public RouletteView View => _view;

        public void Setup(RouletteModel model, AdScheduler adScheduler, IUISoundPlayer soundPlayer,
            ISaveConfirmation saveConfirmation)
        {
            Unsubscribe();
            _model = model;
            _saveConfirmation = saveConfirmation;
            _adScheduler = adScheduler;
            _soundPlayer = soundPlayer;
            _sound = new RouletteSoundFeedback();
            _view.Wheel.Setup(model.Config);
            _model.RefreshSlots();

            if (isActiveAndEnabled == true)
            {
                Subscribe();
            }
        }

        public bool CanSpin()
        {
            return IsBusy() == false && _model.CanSpin(false);
        }

        public bool ShowAndSpin(Action<RoulettePrize> onComplete)
        {
            if (_state != RouletteState.Idle || CanSpin() == false)
            {
                return false;
            }

            Show(onComplete);
            Spin(false);
            return true;
        }

        public void Show(Action<RoulettePrize> onComplete)
        {
            if (_state != RouletteState.Hidden)
            {
                return;
            }

            if (onComplete == null)
            {
                throw new ArgumentNullException(nameof(onComplete));
            }

            _onComplete = onComplete;
            Subscribe();
            _state = RouletteState.Idle;
            _presentationCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy());
            BuildWheel();
            _view.Show();
            RefreshButtons();
        }

        public void Hide()
        {
            if (_state == RouletteState.Hidden)
            {
                return;
            }

            OnViewDisabled();
            _view.Hide();
        }

        public void ResumeAfterPrize()
        {
            if (_state == RouletteState.Hidden)
            {
                return;
            }

            _view.Wheel.Stop();
            _outcome = null;
            _state = RouletteState.Idle;
            BuildWheel();
            RefreshButtons();
        }

        private void OnEnable()
        {
            if (_model != null)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            OnViewDisabled();
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_isSubscribed == true)
            {
                return;
            }

            _view.SpinRequested += OnSpinRequested;
            _view.AdRequested += OnAdRequested;
            _view.CloseRequested += OnCloseRequested;
            _view.Closed += OnViewClosed;
            _view.Disabled += OnViewDisabled;
            _view.Wheel.Completed += OnWheelCompleted;
            _sound.Setup(_view.Wheel, _model.Config, _soundPlayer);
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            _view.SpinRequested -= OnSpinRequested;
            _view.AdRequested -= OnAdRequested;
            _view.CloseRequested -= OnCloseRequested;
            _view.Closed -= OnViewClosed;
            _view.Disabled -= OnViewDisabled;
            _view.Wheel.Completed -= OnWheelCompleted;
            _sound.Release();
            _isSubscribed = false;
        }

        private void Update()
        {
            if (_state == RouletteState.Hidden || Time.unscaledTime < _nextRefreshTime)
            {
                return;
            }

            _nextRefreshTime = Time.unscaledTime + ButtonRefreshInterval;
            RefreshButtons();
        }

        private void OnCloseRequested()
        {
            if (IsBusy() == false)
            {
                CloseRequested?.Invoke();
            }
        }

        private bool IsBusy()
        {
            return _state == RouletteState.WaitingForAd || _state == RouletteState.ConfirmingSave
                || _state == RouletteState.Spinning;
        }

        private void BuildWheel()
        {
            _model.RefreshSlots();
            List<RouletteSectorIcon> icons = new List<RouletteSectorIcon>();

            for (int i = 0; i < _model.Slots.Count; i++)
            {
                RouletteSlot slot = _model.Slots[i];
                Sprite icon;
                string label = null;

                if (slot.IsHidden == true)
                {
                    icon = _view.SecretIcon;
                }
                else if (slot.Skin != null)
                {
                    icon = slot.Skin.Icon;
                }
                else
                {
                    icon = _view.GetCoinIcon(slot.Coins);
                    label = $"×{slot.Coins}";
                }

                icons.Add(new RouletteSectorIcon(icon, label));
            }

            _view.Wheel.Build(icons);
        }

        private void OnSpinRequested()
        {
            if (_state == RouletteState.Idle || _state == RouletteState.SaveFailed)
            {
                Spin(false);
            }
        }

        private void OnAdRequested()
        {
            if (_state != RouletteState.Idle || _model.HasAdvertising == false
                || _model.CanSpin(true) == false)
            {
                return;
            }

            _state = RouletteState.WaitingForAd;
            RefreshButtons();
            _adScheduler.ShowRewarded(_adScheduler.RouletteRewardId, OnAdGranted,
                OnAdNotGranted, OnAdNotGranted, OnAdNotGranted);
        }

        private void OnAdGranted()
        {
            if (_state != RouletteState.WaitingForAd)
            {
                return;
            }

            Spin(true);
        }

        private void OnAdNotGranted()
        {
            if (_state != RouletteState.WaitingForAd)
            {
                return;
            }

            _state = RouletteState.Idle;
            RefreshButtons();
        }

        private void Spin(bool advertisementGranted)
        {
            if (_model.Spin(advertisementGranted, out _outcome) == false)
            {
                _state = RouletteState.Idle;
                RefreshButtons();
                return;
            }

            _state = RouletteState.ConfirmingSave;
            RefreshButtons();
            ConfirmAndSpinAsync(_presentationCancellation.Token).Forget();
        }

        private async UniTaskVoid ConfirmAndSpinAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _saveConfirmation.ConfirmSavedAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                _state = RouletteState.SaveFailed;
                RefreshButtons();
                return;
            }

            _sound.PlaySpin();
            _state = RouletteState.Spinning;
            _view.Wheel.Spin(_outcome.SlotIndex);
        }

        private void OnWheelCompleted()
        {
            RoulettePrize prize = _outcome.Prize;
            Action<RoulettePrize> onComplete = _onComplete;
            ShowPrizeAsync(prize, onComplete, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid ShowPrizeAsync(RoulettePrize prize, Action<RoulettePrize> onComplete,
            CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_model.Config.WinDwellSeconds),
                    DelayType.Realtime, PlayerLoopTiming.Update, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // The prize is granted and saved, so the callback must run even if the popup never shows.
                onComplete?.Invoke(prize);
                return;
            }

            _sound.PlayWin();
            onComplete?.Invoke(prize);
        }

        private void OnViewDisabled()
        {
            _state = RouletteState.Hidden;
            _onComplete = null;
            _outcome = null;

            if (_presentationCancellation != null)
            {
                _presentationCancellation.Cancel();
                _presentationCancellation.Dispose();
                _presentationCancellation = null;
            }

            _view.Wheel.Stop();
        }

        private void OnViewClosed()
        {
            Closed?.Invoke();
        }

        private string FormatRemainingTime(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes}:{seconds:00}";
        }

        private void RefreshButtons()
        {
            string title = Localization.Get("skin_roulette_title");
            string spinText = _model.GetPrice().ToString();

            if (_model.HasAdvertising == true)
            {
                title = Localization.Get("daily_roulette_title");
                int remaining = _model.GetFreeRemainSeconds();
                spinText = Localization.Get("roulette_free_ready");

                if (remaining > 0)
                {
                    spinText = FormatRemainingTime(remaining);
                }
            }

            if (_state == RouletteState.SaveFailed)
            {
                title = Localization.Get("roulette_save_error");
                spinText = Localization.Get("leaderboard_retry");
            }

            RouletteDisplayData data = new RouletteDisplayData
            {
                Title = title,
                ShowSpin = _model.HasAdvertising == true || _state == RouletteState.SaveFailed,
                SpinText = spinText,
                CanSpin = (_state == RouletteState.Idle && _model.CanSpin(false))
                    || _state == RouletteState.SaveFailed,
                ShowAd = _model.HasAdvertising == true && _state != RouletteState.SaveFailed,
                AdText = string.Format(Localization.Get("roulette_ad_spins"), _model.GetAdSpinsLeft()),
                CanWatchAd = _state == RouletteState.Idle && _model.CanSpin(true),
                ShowClose = _model.HasAdvertising,
                CanClose = IsBusy() == false
            };
            _view.Display(data);
        }
    }
}
