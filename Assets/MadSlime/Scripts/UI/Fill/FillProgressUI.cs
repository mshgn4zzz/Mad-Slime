using ShapeFill;
using System;
using TMPro;
using UI.Common;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
namespace UI.Fill
{
    public class FillProgressUI : MonoBehaviour
    {
        [SerializeField] private Image _progressBar;
        [SerializeField] private TMP_Text _percentText;

        private const float FullPercent = 100f;

        private ShapeFiller _shapeFiller;
        private BarFiller _barFiller;

        [Inject]
        public void Construct(ShapeFiller shapeFiller)
        {
            _shapeFiller = shapeFiller;
        }

        private void Awake()
        {
            if (_shapeFiller == null)
            {
                throw new InvalidOperationException(
                    $"{name}: ShapeFiller was not injected. Check that FillLifetimeScope "
                        + $"registers ShapeFiller and FillProgressUI.");
            }

            if (_percentText == null)
            {
                throw new InvalidOperationException($"{name}: assign the fill percentage text.");
            }

            if (_progressBar == null)
            {
                throw new InvalidOperationException($"{name}: assign the fill progress image.");
            }

            _barFiller = new BarFiller(_progressBar);
        }

        private void OnEnable()
        {
            _shapeFiller.CubeArrived += OnCubeArrived;
            _shapeFiller.FillCompleted += OnFillCompleted;
            ApplyProgress(0f);
        }

        private void OnDisable()
        {
            _shapeFiller.CubeArrived -= OnCubeArrived;
            _shapeFiller.FillCompleted -= OnFillCompleted;
        }

        private void OnFillCompleted(float fraction)
        {
            ApplyProgress(fraction);
        }

        private void OnCubeArrived(CubeFlightAnimator cube)
        {
            ApplyProgress(_shapeFiller.GetFormFillFraction());
        }

        private void ApplyProgress(float fraction)
        {
            float fillAmount = Mathf.Clamp01(fraction);
            _barFiller.SetFill(fillAmount);
            int percent = Mathf.RoundToInt(fillAmount * FullPercent);
            _percentText.SetText("{0}%", percent);
        }
    }
}
