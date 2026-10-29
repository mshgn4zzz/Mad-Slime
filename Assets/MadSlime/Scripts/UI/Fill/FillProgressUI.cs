using MadSlime.ShapeFill.Logic;
using MadSlime.ShapeFill.View;
using MadSlime.UI.Common;
using MadSlime.Validation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MadSlime.UI.Fill
{
    public class FillProgressUI : MonoBehaviour
    {
        private const float FullPercent = 100f;
        [SerializeField] private Image _progressBar;
        [SerializeField] private TMP_Text _percentText;

        private ShapeFiller _shapeFiller;
        private BarFiller _barFiller;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_shapeFiller, name, nameof(_shapeFiller));

            ArgumentGuard.CheckAssigned(_percentText, name, nameof(_percentText));

            ArgumentGuard.CheckAssigned(_progressBar, name, nameof(_progressBar));

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

        [Inject]
        public void Construct(ShapeFiller shapeFiller)
        {
            _shapeFiller = shapeFiller;
        }

        private void ApplyProgress(float fraction)
        {
            float fillAmount = Mathf.Clamp01(fraction);

            _barFiller.SetFill(fillAmount);

            int percent = Mathf.RoundToInt(fillAmount * FullPercent);

            _percentText.SetText("{0}%", percent);
        }

        private void OnFillCompleted(float fraction)
        {
            ApplyProgress(fraction);
        }

        private void OnCubeArrived(CubeFlightAnimator cube)
        {
            ApplyProgress(_shapeFiller.GetFormFillFraction());
        }
    }
}
