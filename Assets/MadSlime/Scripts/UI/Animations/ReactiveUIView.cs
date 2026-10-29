using System;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.UI.Animations
{
    public class ReactiveUIView : MonoBehaviour
    {
        [SerializeField] private TweenAnimatorBase[] _animators;

        private CanvasGroup _canvasGroup;
        private bool _isActive;
        private int _pendingAnimations;

        public event Action EnableRequested;

        public event Action DisableRequested;

        public event Action<bool> ResetCompleted;

        public event Action<ReactiveUIView> DisableCompleted;

        public bool IsActive => _isActive;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_animators, name, nameof(_animators));

            TryGetComponent(out _canvasGroup);
        }

        private void OnEnable()
        {
            for (int i = 0; i < _animators.Length; i++)
            {
                ArgumentGuard.CheckAssigned(_animators[i], name, nameof(_animators));

                _animators[i].Completed += OnAnimationCompleted;
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _animators.Length; i++)
            {
                _animators[i].Completed -= OnAnimationCompleted;
            }

            _isActive = false;
            _pendingAnimations = 0;
            SetInteraction(false);
        }

        public void Enable()
        {
            gameObject.SetActive(true);

            if (_isActive)
            {
                return;
            }

            _isActive = true;
            _pendingAnimations = CountActiveAnimators();
            SetInteraction(true);
            EnableRequested?.Invoke();
            CompleteEmptyView();
        }

        public void Disable()
        {
            if (_isActive == false)
            {
                if (_pendingAnimations == 0)
                {
                    NotifyCompleted();
                }

                return;
            }

            _isActive = false;
            _pendingAnimations = CountActiveAnimators();
            SetInteraction(false);
            DisableRequested?.Invoke();
            CompleteEmptyView();
        }

        public void ResetState(bool isActive)
        {
            _isActive = isActive;
            _pendingAnimations = 0;
            SetInteraction(isActive);
            ResetCompleted?.Invoke(isActive);
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
            if (_pendingAnimations == 0)
            {
                NotifyCompleted();
            }
        }

        private int CountActiveAnimators()
        {
            int count = 0;

            for (int i = 0; i < _animators.Length; i++)
            {
                if (_animators[i].isActiveAndEnabled)
                {
                    count++;
                }
            }

            return count;
        }

        private void NotifyCompleted()
        {
            if (_isActive == false)
            {
                DisableCompleted?.Invoke(this);
            }
        }

        private void OnAnimationCompleted()
        {
            if (_pendingAnimations <= 0)
            {
                return;
            }

            _pendingAnimations--;

            if (_pendingAnimations == 0)
            {
                NotifyCompleted();
            }
        }
    }
}
