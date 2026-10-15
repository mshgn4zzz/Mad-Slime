using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    // Keeps covered screens alive while only the top popup receives UI input.
    public sealed class MenuPopupStack
    {
        private sealed class InputState
        {
            private readonly CanvasGroup _group;
            private readonly bool _interactable;
            private readonly bool _blocksRaycasts;

            public InputState(GameObject root)
            {
                if (root.TryGetComponent(out _group) == false)
                {
                    _group = root.AddComponent<CanvasGroup>();
                }

                _interactable = _group.interactable;
                _blocksRaycasts = _group.blocksRaycasts;
            }

            public void Block()
            {
                _group.interactable = false;
                _group.blocksRaycasts = false;
            }

            public void Restore()
            {
                if (_group != null)
                {
                    _group.interactable = _interactable;
                    _group.blocksRaycasts = _blocksRaycasts;
                }
            }
        }

        private readonly GameObject[] _screens;
        private readonly List<Component> _windows = new List<Component>();
        private readonly List<InputState> _popupInput = new List<InputState>();
        private readonly List<InputState> _screenInput = new List<InputState>();

        public bool IsOpen => _windows.Count > 0;

        public MenuPopupStack(params GameObject[] screens)
        {
            _screens = screens;
        }

        public bool IsTop(Component window)
        {
            return IsOpen && _windows[_windows.Count - 1] == window;
        }

        public void Push(Component window)
        {
            if (_windows.Contains(window))
            {
                throw new InvalidOperationException("The popup is already open.");
            }

            if (IsOpen)
            {
                _popupInput[_popupInput.Count - 1].Block();
            }
            else
            {
                foreach (GameObject screen in _screens)
                {
                    InputState input = new InputState(screen);
                    _screenInput.Add(input);
                    input.Block();
                }
            }

            _windows.Add(window);
            _popupInput.Add(new InputState(window.gameObject));
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

            if (IsOpen)
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
            foreach (InputState input in _popupInput)
            {
                input.Restore();
            }

            _windows.Clear();
            _popupInput.Clear();
            RestoreScreens();
        }

        private void RestoreScreens()
        {
            foreach (InputState input in _screenInput)
            {
                input.Restore();
            }

            _screenInput.Clear();
        }
    }
}
