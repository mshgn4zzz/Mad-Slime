using System;
using MadSlime.Roulette.Logic.StateMachine;
using MadSlime.Validation;

namespace MadSlime.UI.Roulette
{
    public class RouletteEventSubscription : IDisposable
    {
        private readonly RoulettePresenter _presenter;
        private readonly RouletteStateMachine _machine;
        private readonly RouletteView _view;

        public RouletteEventSubscription(
            RoulettePresenter presenter, RouletteStateMachine machine, RouletteView view)
        {
            ArgumentGuard.CheckNotNull(presenter, nameof(presenter));
            ArgumentGuard.CheckNotNull(machine, nameof(machine));
            ArgumentGuard.CheckNotNull(view, nameof(view));

            _presenter = presenter;
            _machine = machine;
            _view = view;
            Subscribe();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            _machine.Prepared += _presenter.OnReady;
            _machine.Changed += _presenter.OnMachineChanged;
            _machine.ResultPrepared += _presenter.OnResultReady;
            _machine.AdvertisementRequested += _presenter.OnAdvertisementRequested;
            _machine.CloseRequested += _presenter.OnMachineCloseRequested;
            _machine.Failed += _presenter.OnFailed;
            _view.SpinRequested += _presenter.OnSpinRequested;
            _view.AdRequested += _presenter.OnAdRequested;
            _view.CloseRequested += _presenter.OnCloseRequested;
            _view.Closed += _presenter.OnViewClosed;
            _view.Disabled += _presenter.OnViewDisabled;
        }

        private void Unsubscribe()
        {
            _machine.Prepared -= _presenter.OnReady;
            _machine.Changed -= _presenter.OnMachineChanged;
            _machine.ResultPrepared -= _presenter.OnResultReady;
            _machine.AdvertisementRequested -= _presenter.OnAdvertisementRequested;
            _machine.CloseRequested -= _presenter.OnMachineCloseRequested;
            _machine.Failed -= _presenter.OnFailed;
            _view.SpinRequested -= _presenter.OnSpinRequested;
            _view.AdRequested -= _presenter.OnAdRequested;
            _view.CloseRequested -= _presenter.OnCloseRequested;
            _view.Closed -= _presenter.OnViewClosed;
            _view.Disabled -= _presenter.OnViewDisabled;
        }
    }
}
