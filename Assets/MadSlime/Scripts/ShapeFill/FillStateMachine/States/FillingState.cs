using System;
using Game;

namespace ShapeFill
{
    public class FillingState : IFillState
    {
        private FillSessionData _sessionData;
        private ShapeFillOrchestrator _fillOrchestrator;
        private IGameplayReporter _gameplayReporter;

        public FillingState(
            FillSessionData sessionData,
            ShapeFillOrchestrator fillOrchestrator,
            IGameplayReporter gameplayReporter)
        {
            ArgumentGuard.CheckNotNull(sessionData, nameof(sessionData));
            ArgumentGuard.CheckNotNull(fillOrchestrator, nameof(fillOrchestrator));
            ArgumentGuard.CheckNotNull(gameplayReporter, nameof(gameplayReporter));

            _sessionData = sessionData;
            _fillOrchestrator = fillOrchestrator;
            _gameplayReporter = gameplayReporter;
        }

        public event Action Completed;

        public void Enter()
        {
            _fillOrchestrator.FillCompleted += OnFillCompleted;
            _gameplayReporter.ReportStart();

            switch (_sessionData.StartMode)
            {
                case FillStartMode.AfterRescue:
                    _fillOrchestrator.RescueShow();
                    break;

                case FillStartMode.Initial:
                    _fillOrchestrator.Show(_sessionData.InitialResult);
                    break;
            }
        }

        public void Exit()
        {
            _fillOrchestrator.FillCompleted -= OnFillCompleted;

            _gameplayReporter.ReportStop();
            _fillOrchestrator.Stop();
        }

        private void OnFillCompleted(float percent)
        {
            Completed?.Invoke();
        }

        public void Accelerate()
        {
            _fillOrchestrator.Accelerate();
        }
    }
}
