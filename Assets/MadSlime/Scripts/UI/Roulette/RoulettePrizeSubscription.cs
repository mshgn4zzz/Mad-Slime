using System;
using MadSlime.Validation;

namespace MadSlime.UI.Roulette
{
    public class RoulettePrizeSubscription : IDisposable
    {
        private readonly RouletteWinPopup _popup;
        private readonly Action _closeRequested;
        private readonly Action _closed;

        public RoulettePrizeSubscription(RouletteWinPopup popup, Action closeRequested, Action closed)
        {
            ArgumentGuard.CheckNotNull(popup, nameof(popup));

            _popup = popup;
            _closeRequested = closeRequested;
            _closed = closed;
            _popup.CloseRequested += _closeRequested;
            _popup.Closed += _closed;
        }

        public void Dispose()
        {
            _popup.CloseRequested -= _closeRequested;
            _popup.Closed -= _closed;
        }
    }
}
