using System;
using DG.Tweening;
using UnityEngine;

namespace UI.Animations
{
    public class UiMoveAnimator : TweenAnimatorBase
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private Vector2 _disabledOffset;

        private Vector2 _enabledPosition;

        protected override void Awake()
        {
            base.Awake();

            if (_target == null)
            {
                throw new InvalidOperationException($"{name}: assign the move animation target.");
            }

            _enabledPosition = _target.anchoredPosition;
        }

        protected override Tween CreateEnableTween()
        {
            return DOTween.To(ReadPosition, ApplyPosition, _enabledPosition, ShowDuration);
        }

        protected override Tween CreateDisableTween()
        {
            return DOTween.To(ReadPosition, ApplyPosition, _enabledPosition + _disabledOffset, HideDuration);
        }

        private Vector2 ReadPosition()
        {
            return _target.anchoredPosition;
        }

        private void ApplyPosition(Vector2 position)
        {
            _target.anchoredPosition = position;
        }

        protected override void ApplyEnabledState()
        {
            _target.anchoredPosition = _enabledPosition;
        }

        protected override void ApplyDisabledState()
        {
            _target.anchoredPosition = _enabledPosition + _disabledOffset;
        }
    }
}
