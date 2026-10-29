using System;
using MadSlime.Economy.Logic;
using MadSlime.ShapeFill.FillStateMachine.Interfaces;
using MadSlime.ShapeFill.FillStateMachine.Logic;
using MadSlime.ShapeFill.Logic;
using MadSlime.Validation;

namespace MadSlime.ShapeFill.FillStateMachine.States
{
    public class ResultState : IFillState
    {
        private Rewarder _rewarder;
        private ShapeFillOrchestrator _fillOrchestrator;
        private FillSessionData _sessionData;

        private bool _isRescueAvailable;

        public ResultState(Rewarder rewarder, ShapeFillOrchestrator fillOrchestrator, FillSessionData sessionData)
        {
            ArgumentGuard.CheckNotNull(rewarder, nameof(rewarder));
            ArgumentGuard.CheckNotNull(fillOrchestrator, nameof(fillOrchestrator));
            ArgumentGuard.CheckNotNull(sessionData, nameof(sessionData));

            _rewarder = rewarder;
            _fillOrchestrator = fillOrchestrator;
            _sessionData = sessionData;
        }

        public event Action<int> Succeeded;

        public event Action<int, bool> Failed;
        public event Action RescueRequested;

        public void Enter()
        {
            if (_rewarder.IsWin)
            {
                Succeeded?.Invoke(_rewarder.GrantedTotal);
            }
            else
            {
                _isRescueAvailable = _fillOrchestrator.IsRescueAvailable() && _sessionData.IsRescueUsed == false;
                Failed?.Invoke(_rewarder.GrantedTotal, _isRescueAvailable);
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
