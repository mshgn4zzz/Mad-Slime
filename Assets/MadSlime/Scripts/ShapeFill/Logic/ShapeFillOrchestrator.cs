using System;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.ShapeFill.Logic
{
    [RequireComponent(typeof(GridBuilder))]
    [RequireComponent(typeof(ShapeFiller))]
    public class ShapeFillOrchestrator : MonoBehaviour
    {
        private GridBuilder _gridBuilder;
        private ShapeFiller _shapeFiller;
        private FillResultCalculator _resultCalculator;

        public event Action<float> FillCompleted;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_gridBuilder, name, nameof(_gridBuilder));

            ArgumentGuard.CheckAssigned(_shapeFiller, name, nameof(_shapeFiller));

            ArgumentGuard.CheckAssigned(_resultCalculator, name, nameof(_resultCalculator));
        }

        private void OnEnable()
        {
            _shapeFiller.FillCompleted += OnFillCompleted;
        }

        private void OnDisable()
        {
            if (_shapeFiller != null)
            {
                _shapeFiller.FillCompleted -= OnFillCompleted;
            }
        }

        public bool IsRescueAvailable()
        {
            return _shapeFiller.IsRescueAvailable();
        }

        [Inject]
        public void Construct(
            GridBuilder gridBuilder,
            ShapeFiller shapeFiller,
            FillResultCalculator resultCalculator)
        {
            _gridBuilder = gridBuilder;
            _shapeFiller = shapeFiller;
            _resultCalculator = resultCalculator;
        }

        public void Prepare(Bounds playArea)
        {
            _gridBuilder.SetPlayArea(playArea);
            Prepare();
        }

        public void Prepare()
        {
            _shapeFiller.Initialize();
            _shapeFiller.BuildShape();
        }

        public void SetShapeTexture(Texture2D shapeTexture)
        {
            _gridBuilder.SetShapeTexture(shapeTexture);
        }

        public void FitCamera(GridCameraFitter cameraFitter)
        {
            cameraFitter.Fit(_gridBuilder);
        }

        public FillResult CalculateResult()
        {
            return _resultCalculator.Calculate(
                _gridBuilder.FillCells.Count,
                _gridBuilder.GetFormBounds().center,
                _gridBuilder.GetFormSize());
        }

        public void Show(FillResult result)
        {
            _shapeFiller.ShowResult(result);
        }

        public void Stop()
        {
            _shapeFiller.StopFill();
        }

        public void RescueShow()
        {
            _shapeFiller.Rescue();
        }

        public void Accelerate()
        {
            _shapeFiller.Accelerate();
        }

        private void OnFillCompleted(float fillPercent)
        {
            FillCompleted?.Invoke(fillPercent);
        }
    }
}
