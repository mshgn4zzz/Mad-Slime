using System;
using System.Collections.Generic;
using UI.Spawning;
using UnityEngine;
namespace UI.Menu
{
    public class MenuPopupStack
    {
        private UiSpawner _spawner;
        private GameObject[] _screens;
        private List<Component> _windows = new List<Component>();
        private List<MenuPopupInputState> _popupInput = new List<MenuPopupInputState>();
        private List<MenuPopupInputState> _screenInput = new List<MenuPopupInputState>();

        public bool HasOpenPopups()
        {
            return _windows.Count > 0;
        }

        public MenuPopupStack(UiSpawner spawner, params GameObject[] screens)
        {
            if (spawner == null)
            {
                throw new ArgumentNullException(nameof(spawner));
            }

            if (screens == null)
            {
                throw new ArgumentNullException(nameof(screens));
            }

            for (int i = 0; i < screens.Length; i++)
            {
                if (screens[i] == null)
                {
                    throw new ArgumentNullException(nameof(screens), $"Screen #{i} is not assigned.");
                }
            }

            _spawner = spawner;
            _screens = screens;
        }

        public void Release<T>(T popup, Action<T> detach) where T : Component
        {
            if (popup == null)
            {
                return;
            }

            detach(popup);
            _spawner.Release(popup);
        }

        public bool IsTop(Component window)
        {
            return HasOpenPopups() && _windows[_windows.Count - 1] == window;
        }

        public void Push(Component window)
        {
            if (_windows.Contains(window))
            {
                throw new InvalidOperationException("The popup is already open.");
            }

            if (HasOpenPopups())
            {
                _popupInput[_popupInput.Count - 1].Block();
            }
            else
            {
                foreach (GameObject screen in _screens)
                {
                    MenuPopupInputState input = new MenuPopupInputState(screen);
                    _screenInput.Add(input);
                    input.Block();
                }
            }

            _windows.Add(window);
            _popupInput.Add(new MenuPopupInputState(window.gameObject));
        }

        public void Pop(Component window)
        {
            if (IsTop(window) == false)
            {
                throw new InvalidOperationException("Only the top popup can be removed.");
            }

            int last = _windows.Count - 1;
            _windows.RemoveAt(last);
            _popupInput.RemoveAt(last);

            if (HasOpenPopups())
            {
                _popupInput[_popupInput.Count - 1].Restore();
            }
            else
            {
                RestoreScreens();
            }
        }

        public void Clear()
        {
            foreach (MenuPopupInputState input in _popupInput)
            {
                input.Restore();
            }

            _windows.Clear();
            _popupInput.Clear();
            RestoreScreens();
        }

        private void RestoreScreens()
        {
            foreach (MenuPopupInputState input in _screenInput)
            {
                input.Restore();
            }

            _screenInput.Clear();
        }
    }
}
