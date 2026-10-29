using System;
using MadSlime.ShapeFill.Data;
using MadSlime.ShapeFill.Logic;
using UnityEngine;

namespace MadSlime.ShapeFill.View
{
    [DefaultExecutionOrder(100)]
    public class FillPlayerView : MonoBehaviour
    {
        private const float FullTurn = Mathf.PI * 2f;
        private const float MinimumRadius = 0.001f;
        private const float NearClipPadding = 4f;
        private const float ViewportPadding = 0.02f;

        [SerializeField] private Camera _camera;
        [SerializeField] private GridCameraFitter _cameraFitter;
        [SerializeField] private Transform _animatedRoot;
        [SerializeField] private Transform _modelContainer;

        private FillPlayerConfig _config;
        private Quaternion _initialRotation;
        private Vector3 _angularVelocity;
        private float _modelRadius;
        private float _worldRadius;
        private float _elapsedTime;
        private bool _isAnimating;

        public Vector3 Center => _animatedRoot.position;
        public float Radius => _worldRadius;

        private void LateUpdate()
        {
            if (_config == null)
            {
                return;
            }

            if (_isAnimating)
            {
                _elapsedTime += Time.deltaTime;
            }

            RefreshLayout();
        }

        public void Setup(
            GameObject model,
            FillPlayerConfig config,
            Quaternion initialRotation,
            Vector3 angularVelocity)
        {
            if (_camera == null || _cameraFitter == null || _animatedRoot == null || _modelContainer == null
                || config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: assign the camera, fitter, animation root, model container and config.");
            }

            _config = config;
            _initialRotation = initialRotation;
            _angularVelocity = angularVelocity;
            _elapsedTime = 0f;

            SetModel(model);
        }

        public void SetModel(GameObject model)
        {
            if (model == null || model.transform.parent != _modelContainer)
            {
                throw new InvalidOperationException($"{name}: skin model must belong to the assigned model container.");
            }

            _animatedRoot.localRotation = Quaternion.identity;
            _animatedRoot.localScale = Vector3.one;
            _animatedRoot.localPosition = Vector3.zero;
            _modelContainer.localPosition = Vector3.zero;

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            Bounds localBounds = new Bounds();
            bool isBoundsFound = false;

            foreach (Renderer modelRenderer in renderers)
            {
                Bounds bounds = modelRenderer.localBounds;

                for (int cornerIndex = 0; cornerIndex < 8; cornerIndex++)
                {
                    Vector3 sign = new Vector3(-1f, -1f, -1f);

                    if ((cornerIndex & 1) != 0)
                    {
                        sign.x = 1f;
                    }

                    if ((cornerIndex & 2) != 0)
                    {
                        sign.y = 1f;
                    }

                    if ((cornerIndex & 4) != 0)
                    {
                        sign.z = 1f;
                    }

                    Vector3 worldCorner = modelRenderer.transform.TransformPoint(bounds.center + Vector3.Scale(
                        bounds.extents,
                        sign));
                    Vector3 localCorner = _animatedRoot.InverseTransformPoint(worldCorner);

                    if (isBoundsFound == false)
                    {
                        localBounds = new Bounds(localCorner, Vector3.zero);
                        isBoundsFound = true;
                    }
                    else
                    {
                        localBounds.Encapsulate(localCorner);
                    }
                }
            }

            _modelRadius = localBounds.extents.magnitude;

            if (isBoundsFound == false || _modelRadius < MinimumRadius)
            {
                throw new InvalidOperationException($"{name}: skin model requires non-empty renderer bounds.");
            }

            _modelContainer.localPosition = -localBounds.center;
            RefreshLayout();
        }

        public void SetAnimating(bool isAnimating)
        {
            _isAnimating = isAnimating;
        }

        private void RefreshLayout()
        {
            Rect safeViewport = _cameraFitter.GetSafeViewport();
            Rect formViewport = _cameraFitter.GetFormViewport();
            float diameterHeight = Mathf.Min(
                _config.ViewportWidth * safeViewport.width * _camera.aspect,
                _config.MaxViewportHeight * safeViewport.height);

            diameterHeight *= _config.SizeMultiplier;

            float maximumDiameterHeight = formViewport.yMin - safeViewport.yMin
                - (ViewportPadding * 2f * safeViewport.height);
            float maximumDiameterWidth = safeViewport.width * (1f - (ViewportPadding * 2f)) * _camera.aspect;

            diameterHeight = Mathf.Min(diameterHeight, maximumDiameterHeight, maximumDiameterWidth);

            float depth = Mathf.Max(
                _camera.nearClipPlane * NearClipPadding,
                _cameraFitter.Distance * _config.CameraDepthFraction);
            float halfHeight = _camera.orthographicSize;

            if (_camera.orthographic == false)
            {
                halfHeight = depth * Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
            }

            float radius = diameterHeight * halfHeight;
            float maximumMotionScale = 1f + _config.PulseAmplitude + _config.HoverAmplitude;

            radius /= maximumMotionScale;

            if (_camera.orthographic == false)
            {
                radius /= Mathf.Sqrt(1f + ((radius * radius) / (depth * depth)));
            }

            Vector3 viewportPosition = new Vector3(
                safeViewport.center.x,
                (safeViewport.yMin + formViewport.yMin) * 0.5f,
                depth);
            float pulse = 1f + (Mathf.Sin(_elapsedTime * FullTurn * _config.PulseFrequency) * _config.PulseAmplitude);
            float hover = Mathf.Sin(_elapsedTime * FullTurn * _config.HoverFrequency) * _config.HoverAmplitude * radius;

            transform.SetPositionAndRotation(
                _camera.ViewportToWorldPoint(viewportPosition),
                _camera.transform.rotation);
            _animatedRoot.localPosition = Vector3.up * hover;
            _animatedRoot.localRotation = _initialRotation * Quaternion.Euler(_angularVelocity * _elapsedTime);
            _animatedRoot.localScale = Vector3.one * (radius * pulse / _modelRadius);
            _worldRadius = radius * pulse;
        }
    }
}
