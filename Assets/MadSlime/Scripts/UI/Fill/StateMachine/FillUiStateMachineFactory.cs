using MadSlime.Economy.Logic;
using MadSlime.Saves.Interfaces;

namespace MadSlime.UI.Fill.StateMachine
{
    public class FillUiStateMachineFactory
    {
        private readonly DoubleRewardModel _rewards;
        private readonly ISaveConfirmation _confirmation;

        public FillUiStateMachineFactory(DoubleRewardModel rewards, ISaveConfirmation confirmation)
        {
            _rewards = rewards;
            _confirmation = confirmation;
        }

        public FillUiStateMachine Create()
        {
            return new FillUiStateMachine(_rewards, _confirmation);
        }
    }
}
