using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Ads.Logic;
using MadSlime.Audio.Interfaces;
using MadSlime.Roulette.Logic;
using MadSlime.Roulette.Logic.StateMachine;
using MadSlime.UI.Menu;
using MadSlime.UI.Spawning;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.UI.Roulette
{
    public class RoulettePresenter : MonoBehaviour
    {
        private const float ButtonRefreshInterval = 1f;
        [VContainer.Inject] private RouletteDisplayBuilder _displayBuilder;

        [SerializeField] private RouletteView _view;

        private RouletteEventSubscription _events;
        private RoulettePrizeSubscription _prizeSubscription;
        private RouletteStateMachine _machine;
        private AdScheduler _ads;
        private IUISoundPlayer _soundPlayer;
        private RouletteSoundFeedback _sound;
        private UiSpawner _spawner;
        private MenuPopupStack _popups;
        private RouletteWinPopup _prize;
        private CancellationTokenSource _presentationCancellation;
        private float _nextRefreshTime;

        public event Action Closed;
        public event Action<RoulettePrize> PrizePresented;

        private void OnEnable()
        {
            Attach();
        }

        private void OnDisable()
        {
            OnViewDisabled();
            Detach();
        }

        private void OnDestroy()
        {
            Detach();
            _machine?.Dispose();
        }

        private void Update()
        {
            if (_machine == null || _machine.IsRunning() == false || Time.unscaledTime < _nextRefreshTime)
            {
                return;
            }

            _nextRefreshTime = Time.unscaledTime + ButtonRefreshInterval;
            OnMachineChanged();
        }

        public void Setup(
            RouletteStateMachine machine,
            AdScheduler ads,
            IUISoundPlayer soundPlayer,
            UiSpawner spawner,
            MenuPopupStack popups)
        {
            ArgumentGuard.CheckNotNull(machine, nameof(machine));
            ArgumentGuard.CheckNotNull(ads, nameof(ads));
            ArgumentGuard.CheckNotNull(soundPlayer, nameof(soundPlayer));
            ArgumentGuard.CheckNotNull(spawner, nameof(spawner));
            ArgumentGuard.CheckNotNull(popups, nameof(popups));
            Detach();
            StopPresentation();
            _machine?.Dispose();
            _machine = machine;
            _ads = ads;
            _soundPlayer = soundPlayer;
            _spawner = spawner;
            _popups = popups;
            _sound = new RouletteSoundFeedback();
            _view.Setup(machine.Model.Config);

            if (isActiveAndEnabled)
            {
                Attach();
            }
        }

        public void Show()
        {
            if (_machine.IsRunning() || _presentationCancellation != null)
            {
                return;
            }

            _presentationCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy());
            _view.Show();
            _machine.Start();
        }

        public bool TryShowAndSpin()
        {
            if (_machine.IsRunning() || _presentationCancellation != null
                || _machine.Model.IsSpinAvailable(false) == false)
            {
                return false;
            }

            Show();
            return _machine.TrySpin();
        }

        public void Hide()
        {
            if (_machine == null || _machine.IsRunning() == false)
            {
                return;
            }

            _machine.Stop();
            CancelPresentation();
            OnMachineChanged();
            _view.Hide();
        }

        private void Attach()
        {
            if (_machine == null)
            {
                return;
            }

            _events = new RouletteEventSubscription(this, _machine, _view);
            _sound.Setup(_view.Wheel, _machine.Model.Config, _soundPlayer);
        }

        private void Detach()
        {
            if (_machine == null)
            {
                return;
            }

            _events?.Dispose();
            _events = null;
            _sound.Release();
        }

        private async UniTask PresentResultAsync(RouletteOutcome outcome, CancellationToken token)
        {
            try
            {
                _sound.PlaySpin();
                await _view.PresentResultAsync(outcome.SlotIndex, token);
                token.ThrowIfCancellationRequested();
                _sound.PlayWin();
                _prize = _spawner.Spawn(_view.WinPopupPrefab, UiLayer.Popup);
                _prize.Initialize(outcome.Prize, _view);
                _prizeSubscription = new RoulettePrizeSubscription(
                    _prize, OnPrizeCloseRequested, OnPrizeClosed);
                _popups.Push(_prize);
                _spawner.Show(_prize);
                PrizePresented?.Invoke(outcome.Prize);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                OnFailed(exception);
                Hide();
            }
        }

        private void ReleasePrize()
        {
            RouletteWinPopup prize = _prize;

            _prize = null;
            _prizeSubscription?.Dispose();
            _prizeSubscription = null;

            if (prize == null)
            {
                return;
            }

            if (_popups.IsTop(prize))
            {
                _popups.Pop(prize);
            }

            _spawner.Release(prize);
        }

        private void CancelPresentation()
        {
            _presentationCancellation?.Cancel();
            ReleasePrize();
            _view.Wheel.Stop();
        }

        private void StopPresentation()
        {
            CancellationTokenSource source = _presentationCancellation;

            _presentationCancellation = null;
            source?.Cancel();
            source?.Dispose();
            ReleasePrize();
            _view.Wheel.Stop();
        }

        internal void OnMachineCloseRequested()
        {
            Hide();
        }

        internal void OnMachineChanged()
        {
            _view.Display(_displayBuilder.Build(_machine));
        }

        internal void OnReady()
        {
            _view.Wheel.Stop();
            _displayBuilder.BuildWheel(_machine.Model, _view);
        }

        internal void OnSpinRequested()
        {
            _machine.TrySpin();
        }

        internal void OnAdRequested()
        {
            _machine.TryAdvertisement();
        }

        internal void OnCloseRequested()
        {
            _machine.RequestClose();
        }

        internal void OnAdvertisementRequested(Action<bool> complete)
        {
            _ads.ShowRewarded(
                _ads.RouletteRewardId,
                () => complete(true),
                () => complete(false),
                () => complete(false),
                () => complete(false));
        }

        internal void OnResultReady(RouletteOutcome outcome)
        {
            PresentResultAsync(outcome, _presentationCancellation.Token).Forget();
        }

        internal void OnViewDisabled()
        {
            bool isPresentationAssigned = _presentationCancellation != null;

            _machine?.Stop();
            StopPresentation();

            if (isPresentationAssigned)
            {
                Closed?.Invoke();
            }
        }

        internal void OnViewClosed()
        {
            _machine.Stop();
            StopPresentation();
            Closed?.Invoke();
        }

        internal void OnFailed(Exception exception)
        {
            UniTask.FromException(exception).Forget();
        }

        private void OnPrizeCloseRequested()
        {
            if (_prize != null && _popups.IsTop(_prize))
            {
                _prize.BeginClose();
            }
        }

        private void OnPrizeClosed()
        {
            ReleasePrize();
            _machine.CompletePresentation();
        }
    }
}
