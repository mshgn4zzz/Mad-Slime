using System;
using DG.Tweening;
using UnityEngine;

namespace UI.Animations
{
    [RequireComponent(typeof(ReactiveUIView))]
    public abstract class TweenAnimatorBase : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _showDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float _hideDuration = 0.2f;
        [SerializeField] private Ease _showEase = Ease.OutCubic;
        [SerializeField] private Ease _hideEase = Ease.InCubic;

        private ReactiveUIView _view;
        private Tween _currentTween;

        public event Action Completed;

        protected float ShowDuration => _showDuration;
        protected float HideDuration => _hideDuration;

        protected virtual void Awake()
        {
            if (TryGetComponent(out _view) == false)
            {
                throw new InvalidOperationException($"{name}: ReactiveUIView is required.");
            }
        }

        protected virtual void OnEnable()
        {
            _view.EnableRequested += OnEnableRequested;
            _view.DisableRequested += OnDisableRequested;
            _view.StateReset += OnStateReset;
            OnStateReset(_view.IsActive);
        }

        protected virtual void OnDisable()
        {
            _view.EnableRequested -= OnEnableRequested;
            _view.DisableRequested -= OnDisableRequested;
            _view.StateReset -= OnStateReset;
            KillTween();
        }

        protected abstract Tween CreateEnableTween();
        protected abstract Tween CreateDisableTween();
        protected abstract void ApplyEnabledState();
        protected abstract void ApplyDisabledState();

        private void OnEnableRequested()
        {
            KillTween();
            _currentTween = CreateEnableTween();
            _currentTween.SetEase(_showEase).SetUpdate(true).SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .OnComplete(OnCompleted);
        }

        private void OnDisableRequested()
        {
            KillTween();
            _currentTween = CreateDisableTween();
            _currentTween.SetEase(_hideEase).SetUpdate(true).SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .OnComplete(OnCompleted);
        }

        private void OnCompleted()
        {
            _currentTween = null;
            Completed?.Invoke();
        }

        private void OnStateReset(bool isActive)
        {
            KillTween();

            if (isActive == true)
            {
                ApplyEnabledState();
            }
            else
            {
                ApplyDisabledState();
            }
        }

        private void KillTween()
        {
            if (_currentTween != null)
            {
                _currentTween.Kill();
                _currentTween = null;
            }
        }
    }
}
