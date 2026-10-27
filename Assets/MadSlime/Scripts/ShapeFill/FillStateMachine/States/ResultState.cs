using System;
using Economy;
using Game;

namespace ShapeFill
{
    public class ResultState : IFillState
    {
        private Rewarder _rewarder;
        private ShapeFillOrchestrator _fillOrchestrator;
        private FillSessionData _sessionData;

        public ResultState(Rewarder rewarder, ShapeFillOrchestrator fillOrchestrator, FillSessionData sessionData)
        {
            ArgumentGuard.CheckNotNull(rewarder, nameof(rewarder));
            ArgumentGuard.CheckNotNull(fillOrchestrator, nameof(fillOrchestrator));
            ArgumentGuard.CheckNotNull(sessionData, nameof(sessionData));

            _rewarder = rewarder;
            _fillOrchestrator = fillOrchestrator;
            _sessionData = sessionData;
        }

        public event Action<int> Win;

        public event Action<int, bool> Failed;
        public event Action RescueRequested;

        private bool _canRescue;

        public void Enter()
        {
            if (_rewarder.IsWin)
            {
                Win?.Invoke(_rewarder.GrantedTotal);
            }
            else
            {
                _canRescue = _fillOrchestrator.CanRescue() && _sessionData.IsRescueUsed == false;
                Failed?.Invoke(_rewarder.GrantedTotal, _canRescue);
            }
        }

        public void Exit()
        {
        }

        public void RequestRescue()
        {
            RescueRequested?.Invoke();
        }
    }
}
