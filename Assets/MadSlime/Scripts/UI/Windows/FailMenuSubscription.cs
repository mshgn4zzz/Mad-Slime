using System;
using MadSlime.Validation;

namespace MadSlime.UI.Windows
{
    public class FailMenuSubscription : IDisposable
    {
        private readonly FailMenu _menu;
        private readonly Action _rescueRequested;
        private readonly Action _restartRequested;
        private readonly Action _menuRequested;
        private readonly Action _closed;

        public FailMenuSubscription(
            FailMenu menu,
            Action rescueRequested,
            Action restartRequested,
            Action menuRequested,
            Action closed)
        {
            ArgumentGuard.CheckNotNull(menu, nameof(menu));

            _menu = menu;
            _rescueRequested = rescueRequested;
            _restartRequested = restartRequested;
            _menuRequested = menuRequested;
            _closed = closed;
            Subscribe();
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            _menu.RescueRequested += _rescueRequested;
            _menu.RestartRequested += _restartRequested;
            _menu.MenuRequested += _menuRequested;
            _menu.Closed += _closed;
        }

        private void Unsubscribe()
        {
            _menu.RescueRequested -= _rescueRequested;
            _menu.RestartRequested -= _restartRequested;
            _menu.MenuRequested -= _menuRequested;
            _menu.Closed -= _closed;
        }
    }
}
