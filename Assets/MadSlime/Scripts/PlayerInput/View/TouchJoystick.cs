using System;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

namespace MadSlime.PlayerInput.View
{
    public class TouchJoystick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;

        [SerializeField] private float _movementRange = 120f;

        private RectTransform _zone;
        private Vector2 _dragOrigin;
        private int _activePointerId;
        private bool _isDragging;

        protected override string controlPathInternal { get; set; } = "<Gamepad>/leftStick";

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_isDragging)
            {
                return;
            }

            if (IsTouchAvailable() == false)
            {
                return;
            }

            if (TryGetLocalPoint(eventData, out Vector2 localPoint) == false)
            {
                return;
            }

            _isDragging = true;
            _activePointerId = eventData.pointerId;

            _background.anchoredPosition = localPoint;
            _handle.anchoredPosition = Vector2.zero;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background, eventData.position, eventData.pressEventCamera, out _dragOrigin) == false)
            {
                _dragOrigin = Vector2.zero;
            }

            ShowVisuals();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging == false || eventData.pointerId != _activePointerId)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint) == false)
            {
                return;
            }

            Vector2 canvasDelta = Vector2.Scale(localPoint - _dragOrigin, _background.localScale);
            float movementRange = _movementRange * Mathf.Min(_background.localScale.x, _background.localScale.y);
            Vector2 clampedDelta = Vector2.ClampMagnitude(canvasDelta, movementRange);

            _handle.anchoredPosition = InverseScale(clampedDelta, _background.localScale);

            SendValueToControl(clampedDelta / movementRange);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isDragging == false || eventData.pointerId != _activePointerId)
            {
                return;
            }

            EndDrag();
        }

        protected override void OnDisable()
        {
            EndDrag();

            base.OnDisable();
        }

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_background, name, nameof(_background));

            ArgumentGuard.CheckAssigned(_handle, name, nameof(_handle));

            if ((transform is RectTransform) == false)
            {
                throw new InvalidOperationException(
                    $"{name}: TouchJoystick must be attached to a UI element with a RectTransform.");
            }

            ArgumentGuard.CheckAssigned(_canvas, name, nameof(_canvas));

            ArgumentGuard.CheckPositive(_movementRange, nameof(_movementRange));

            _zone = (RectTransform)transform;

            HideVisuals();
        }

        private bool IsTouchAvailable()
        {
            if (Application.isEditor)
            {
                return true;
            }

            return Touchscreen.current != null;
        }

        private Vector2 InverseScale(Vector2 value, Vector3 scale)
        {
            return new Vector2(value.x / scale.x, value.y / scale.y);
        }

        private bool TryGetLocalPoint(PointerEventData eventData, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _zone, eventData.position, eventData.pressEventCamera, out localPoint);
        }

        private void ShowVisuals()
        {
            _background.gameObject.SetActive(true);
        }

        private void HideVisuals()
        {
            _background.gameObject.SetActive(false);
        }

        private void EndDrag()
        {
            if (_isDragging == false)
            {
                return;
            }

            _isDragging = false;

            SendValueToControl(Vector2.zero);

            HideVisuals();
        }
    }
}
