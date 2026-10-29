using System;
using MadSlime.Validation;

namespace MadSlime.UI.Windows
{
    public class PauseMenuSubscription : IDisposable
    {
        private readonly PauseMenu _menu;
        private readonly Action _closeRequested;
        private readonly Action _menuRequested;
        private readonly Action _closed;

        public PauseMenuSubscription(
            PauseMenu menu,
            Action closeRequested,
            Action menuRequested,
            Action closed)
        {
            ArgumentGuard.CheckNotNull(menu, nameof(menu));

            _menu = menu;
            _closeRequested = closeRequested;
            _menuRequested = menuRequested;
            _closed = closed;
            Subscribe();
        }

        public void DisableInput()
        {
            Unsubscribe(true);
        }

        public void Dispose()
        {
            Unsubscribe(false);
        }

        private void Subscribe()
        {
            _menu.CloseButtonClicked += _closeRequested;
            _menu.MenuButtonClicked += _menuRequested;
            _menu.Closed += _closed;
        }

        private void Unsubscribe(bool isKeepClosed)
        {
            _menu.CloseButtonClicked -= _closeRequested;
            _menu.MenuButtonClicked -= _menuRequested;

            if (isKeepClosed == false)
            {
                _menu.Closed -= _closed;
            }
        }
    }
}
