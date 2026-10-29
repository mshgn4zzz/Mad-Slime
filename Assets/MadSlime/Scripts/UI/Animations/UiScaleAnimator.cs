using System;
using DG.Tweening;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.UI.Animations
{
    public class UiScaleAnimator : TweenAnimatorBase
    {
        private const float BoundsFillRatio = 0.92f;

        [SerializeField] private Transform _target;
        [SerializeField] private RectTransform _bounds;
        [SerializeField, Min(0f)] private float _enabledScale = 1f;
        [SerializeField, Min(0f)] private float _disabledScale;

        private Vector3 _baseScale;
        private RectTransform _targetRect;
        private bool _isScaleRefreshRequired;

        protected override void Awake()
        {
            base.Awake();

            ArgumentGuard.CheckAssigned(_target, name, nameof(_target));

            _baseScale = _target.localScale;

            if (_bounds != null)
            {
                if (_target.TryGetComponent(out _targetRect) == false)
                {
                    throw new InvalidOperationException($"{name}: fitting to bounds requires a RectTransform target.");
                }

                _isScaleRefreshRequired = true;
            }
        }

        private void LateUpdate()
        {
            if (_isScaleRefreshRequired == false || View == null || View.IsActive == false || IsAnimating())
            {
                return;
            }

            _isScaleRefreshRequired = false;
            ApplyEnabledState();
        }

        private void OnRectTransformDimensionsChange()
        {
            _isScaleRefreshRequired = _bounds != null;
        }

        protected override Tween CreateEnableTween()
        {
            return _target.DOScale(CalculateEnabledScale(), ShowDuration);
        }

        protected override Tween CreateDisableTween()
        {
            return _target.DOScale(_baseScale * _disabledScale, HideDuration);
        }

        protected override void ApplyEnabledState()
        {
            _target.localScale = CalculateEnabledScale();
        }

        protected override void ApplyDisabledState()
        {
            _target.localScale = _baseScale * _disabledScale;
        }

        private Vector3 CalculateEnabledScale()
        {
            Vector3 scale = _baseScale * _enabledScale;

            if (_bounds == null || _targetRect == null)
            {
                return scale;
            }

            float width = _targetRect.rect.width * scale.x;
            float height = _targetRect.rect.height * scale.y;

            if (width <= 0f || height <= 0f || _bounds.rect.width <= 0f || _bounds.rect.height <= 0f)
            {
                return scale;
            }

            float horizontalFit = _bounds.rect.width * BoundsFillRatio / width;
            float verticalFit = _bounds.rect.height * BoundsFillRatio / height;
            float fit = Mathf.Min(1f, Mathf.Min(horizontalFit, verticalFit));

            return scale * fit;
        }
    }
}
