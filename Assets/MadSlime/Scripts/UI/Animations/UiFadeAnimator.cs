using System;
using DG.Tweening;
using UnityEngine;

namespace UI.Animations
{
    public class UiFadeAnimator : TweenAnimatorBase
    {
        [SerializeField] private CanvasGroup _target;
        [SerializeField, Range(0f, 1f)] private float _enabledAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float _disabledAlpha;

        protected override void Awake()
        {
            base.Awake();

            if (_target == null)
            {
                throw new InvalidOperationException($"{name}: assign the fade animation target.");
            }
        }

        protected override Tween CreateEnableTween()
        {
            return DOTween.To(ReadAlpha, ApplyAlpha, _enabledAlpha, ShowDuration);
        }

        protected override Tween CreateDisableTween()
        {
            return DOTween.To(ReadAlpha, ApplyAlpha, _disabledAlpha, HideDuration);
        }

        private float ReadAlpha()
        {
            return _target.alpha;
        }

        private void ApplyAlpha(float alpha)
        {
            _target.alpha = alpha;
        }

        protected override void ApplyEnabledState()
        {
            _target.alpha = _enabledAlpha;
        }

        protected override void ApplyDisabledState()
        {
            _target.alpha = _disabledAlpha;
        }
    }
}
