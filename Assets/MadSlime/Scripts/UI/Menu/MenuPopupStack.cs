using System;
using System.Collections.Generic;
using MadSlime.UI.Spawning;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.UI.Menu
{
    public class MenuPopupStack
    {
        private MenuPopupInputFactory _inputFactory = new MenuPopupInputFactory();
        private UiSpawner _spawner;
        private GameObject[] _screens;
        private List<Component> _windows = new List<Component>();
        private List<MenuPopupInputState> _popupInput = new List<MenuPopupInputState>();
        private List<MenuPopupInputState> _screenInput = new List<MenuPopupInputState>();

        public MenuPopupStack(UiSpawner spawner, params GameObject[] screens)
        {
            ArgumentGuard.CheckNotNull(spawner, nameof(spawner));

            ArgumentGuard.CheckNotNull(screens, nameof(screens));

            for (int i = 0; i < screens.Length; i++)
            {
                ArgumentGuard.CheckNotNull(screens[i], nameof(screens));
            }

            _spawner = spawner;
            _screens = screens;
        }

        public bool IsPopupOpen()
        {
            return _windows.Count > 0;
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
            return IsPopupOpen() && _windows[_windows.Count - 1] == window;
        }

        public void Push(Component window)
        {
            if (_windows.Contains(window))
            {
                throw new InvalidOperationException("The popup is already open.");
            }

            if (IsPopupOpen())
            {
                _popupInput[_popupInput.Count - 1].Block();
            }
            else
            {
                foreach (GameObject screen in _screens)
                {
                    MenuPopupInputState input = _inputFactory.Create(screen);

                    _screenInput.Add(input);
                    input.Block();
                }
            }

            _windows.Add(window);
            _popupInput.Add(_inputFactory.Create(window.gameObject));
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

            if (IsPopupOpen())
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
