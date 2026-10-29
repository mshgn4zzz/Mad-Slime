using System;
using UnityEngine;
using UnityEngine.UI;

namespace MadSlime.UI.ResponsiveUI.Runtime
{
    [DefaultExecutionOrder(-2000), DisallowMultipleComponent]
    public class ResponsiveCanvasLayout : MonoBehaviour
    {
        private const float PortraitWidth = 1080f;
        private const float PortraitHeight = 1920f;

        [SerializeField] private CanvasScaler _scaler;
        [SerializeField] private CanvasElement[] _elements = Array.Empty<CanvasElement>();
        [SerializeField] private CanvasVisibility[] _visibility = Array.Empty<CanvasVisibility>();

        private Vector2Int _screen;

        private void Awake()
        {
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void Update()
        {
            if (_screen.x != Screen.width || _screen.y != Screen.height)
            {
                Apply();
            }
        }

        public bool IsManaging(RectTransform target)
        {
            foreach (CanvasElement element in _elements)
            {
                if (element.Target == target)
                {
                    return true;
                }
            }

            return false;
        }

        public void Apply()
        {
            _screen = new Vector2Int(Screen.width, Screen.height);
            ApplyOrientation(_screen.x > _screen.y);
        }

        public void ApplyOrientation(bool isLandscape)
        {
            if (_scaler != null)
            {
                _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

                Vector2 referenceResolution = new Vector2(PortraitWidth, PortraitHeight);

                if (isLandscape)
                {
                    referenceResolution = new Vector2(PortraitHeight, PortraitWidth);
                }

                _scaler.referenceResolution = referenceResolution;
            }

            foreach (CanvasElement element in _elements)
            {
                if (element.Target == null)
                {
                    continue;
                }

                Transform parent = element.PortraitParent;
                CanvasPlacement placement = element.Portrait;

                if (isLandscape)
                {
                    parent = element.LandscapeParent;
                    placement = element.Landscape;
                }

                if (parent != null && element.Target.parent != parent)
                {
                    element.Target.SetParent(parent, false);
                    element.Target.SetAsLastSibling();
                }

                placement.Apply(element.Target);
            }

            foreach (CanvasVisibility entry in _visibility)
            {
                if (entry.Target == null)
                {
                    continue;
                }

                bool isVisible = entry.IsPortraitVisible;

                if (isLandscape)
                {
                    isVisible = entry.IsLandscapeVisible;
                }

                entry.Target.SetActive(isVisible);
            }
        }
    }
}
