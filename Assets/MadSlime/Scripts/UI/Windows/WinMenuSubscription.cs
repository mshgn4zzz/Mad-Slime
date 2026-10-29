using System;
using MadSlime.Validation;

namespace MadSlime.UI.Windows
{
    public class WinMenuSubscription : IDisposable
    {
        private readonly WinMenu _menu;
        private readonly Action _nextLevelRequested;
        private readonly Action _doubleRewardRequested;
        private readonly Action _menuRequested;
        private readonly Action _closed;

        public WinMenuSubscription(
            WinMenu menu,
            Action nextLevelRequested,
            Action doubleRewardRequested,
            Action menuRequested,
            Action closed)
        {
            ArgumentGuard.CheckNotNull(menu, nameof(menu));

            _menu = menu;
            _nextLevelRequested = nextLevelRequested;
            _doubleRewardRequested = doubleRewardRequested;
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
            _menu.NextLevelRequested += _nextLevelRequested;
            _menu.DoubleRewardRequested += _doubleRewardRequested;
            _menu.MenuRequested += _menuRequested;
            _menu.Closed += _closed;
        }

        private void Unsubscribe()
        {
            _menu.NextLevelRequested -= _nextLevelRequested;
            _menu.DoubleRewardRequested -= _doubleRewardRequested;
            _menu.MenuRequested -= _menuRequested;
            _menu.Closed -= _closed;
        }
    }
}
