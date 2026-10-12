using System;
using UnityEngine;

namespace UI.Animations
{
    public class ReactiveUIView : MonoBehaviour
    {
        [SerializeField] private TweenAnimatorBase[] _animators;

        private CanvasGroup _canvasGroup;
        private bool _isActive;
        private int _pendingAnimations;

        public event Action EnableRequested;
        public event Action DisableRequested;
        public event Action<bool> StateReset;
        public event Action<ReactiveUIView> EnableCompleted;
        public event Action<ReactiveUIView> DisableCompleted;

        public bool IsActive => _isActive;

        private void Awake()
        {
            if (_animators == null)
            {
                throw new InvalidOperationException($"{name}: assign the view animators in the prefab.");
            }

            TryGetComponent(out _canvasGroup);
        }

        private void OnEnable()
        {
            for (int i = 0; i < _animators.Length; i++)
            {
                if (_animators[i] == null)
                {
                    throw new InvalidOperationException($"{name}: animator {i} is missing.");
                }

                _animators[i].Completed += OnAnimationCompleted;
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _animators.Length; i++)
            {
                _animators[i].Completed -= OnAnimationCompleted;
            }
        }

        public void Enable()
        {
            gameObject.SetActive(true);

            if (_isActive == true)
            {
                return;
            }

            _isActive = true;
            _pendingAnimations = _animators.Length;
            SetInteraction(true);
            EnableRequested?.Invoke();
            CompleteEmptyView();
        }

        public void Disable()
        {
            if (_isActive == false)
            {
                return;
            }

            _isActive = false;
            _pendingAnimations = _animators.Length;
            SetInteraction(false);
            DisableRequested?.Invoke();
            CompleteEmptyView();
        }

        public void ResetState(bool isActive)
        {
            _isActive = isActive;
            _pendingAnimations = 0;
            SetInteraction(isActive);
            StateReset?.Invoke(isActive);
        }

        private void SetInteraction(bool isActive)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = isActive;
                _canvasGroup.blocksRaycasts = isActive;
            }
        }

        private void CompleteEmptyView()
        {
            if (_animators.Length == 0)
            {
                NotifyCompleted();
            }
        }

        private void OnAnimationCompleted()
        {
            _pendingAnimations--;

            if (_pendingAnimations == 0)
            {
                NotifyCompleted();
            }
        }

        private void NotifyCompleted()
        {
            if (_isActive == true)
            {
                EnableCompleted?.Invoke(this);
            }
            else
            {
                DisableCompleted?.Invoke(this);
            }
        }
    }
}
