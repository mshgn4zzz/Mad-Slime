using System;
using UnityEngine;

namespace MadSlime.UI.ResponsiveUI.Runtime
{
    [DisallowMultipleComponent, RequireComponent(typeof(ResponsiveCanvasLayout))]
    public class RoulettePopupLayout : MonoBehaviour
    {
        [SerializeField]
        private CanvasElement[] _withoutControls =
            Array.Empty<CanvasElement>();

        [SerializeField] private RectTransform _spinButton;
        [SerializeField] private RectTransform _adButton;

        private bool _isSpinVisible;
        private bool _isAdVisible;
        private Vector2Int _screen;
        private ResponsiveCanvasLayout _profiles;

        private void Update()
        {
            if (_screen.x != Screen.width || _screen.y != Screen.height)
            {
                Apply();
            }
        }

        public void SetControlsVisible(bool isSpinVisible, bool isAdvertisementVisible)
        {
            _isSpinVisible = isSpinVisible;
            _isAdVisible = isAdvertisementVisible;
            Apply();
        }

        public void ApplyOrientation(bool isLandscape, bool isWithoutControls)
        {
            EnsureProfiles();
            _profiles.ApplyOrientation(isLandscape);

            if (isWithoutControls == false)
            {
                return;
            }

            foreach (CanvasElement element in _withoutControls)
            {
                if (element.Target == null)
                {
                    continue;
                }

                CanvasPlacement placement = element.Portrait;

                if (isLandscape)
                {
                    placement = element.Landscape;
                }

                placement.Apply(element.Target);
            }
        }

        private void Apply()
        {
            _screen = new Vector2Int(Screen.width, Screen.height);
            EnsureProfiles();
            ApplyOrientation(_screen.x > _screen.y, _isSpinVisible == false && _isAdVisible == false);
            CenterSingleControl();
        }

        private void EnsureProfiles()
        {
            if (_profiles != null)
            {
                return;
            }

            if (TryGetComponent(out _profiles) == false)
            {
                throw new InvalidOperationException($"{name}: ResponsiveCanvasLayout is required.");
            }
        }

        private void CenterSingleControl()
        {
            if (_isSpinVisible && _isAdVisible == false && _spinButton != null)
            {
                _spinButton.anchoredPosition = new Vector2(0f, _spinButton.anchoredPosition.y);
            }

            if (_isAdVisible && _isSpinVisible == false && _adButton != null)
            {
                _adButton.anchoredPosition = new Vector2(0f, _adButton.anchoredPosition.y);
            }
        }
    }
}
