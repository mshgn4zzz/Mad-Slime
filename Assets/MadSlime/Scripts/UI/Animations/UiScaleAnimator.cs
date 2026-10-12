using System;
using DG.Tweening;
using UnityEngine;

namespace UI.Animations
{
    public class UiScaleAnimator : TweenAnimatorBase
    {
        [SerializeField] private Transform _target;
        [SerializeField, Min(0f)] private float _enabledScale = 1f;
        [SerializeField, Min(0f)] private float _disabledScale;

        private Vector3 _baseScale;

        protected override void Awake()
        {
            base.Awake();

            if (_target == null)
            {
                throw new InvalidOperationException($"{name}: assign the scale animation target.");
            }

            _baseScale = _target.localScale;
        }

        protected override Tween CreateEnableTween()
        {
            return _target.DOScale(_baseScale * _enabledScale, ShowDuration);
        }

        protected override Tween CreateDisableTween()
        {
            return _target.DOScale(_baseScale * _disabledScale, HideDuration);
        }

        protected override void ApplyEnabledState()
        {
            _target.localScale = _baseScale * _enabledScale;
        }

        protected override void ApplyDisabledState()
        {
            _target.localScale = _baseScale * _disabledScale;
        }
    }
}
