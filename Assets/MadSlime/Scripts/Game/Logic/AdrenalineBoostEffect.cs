using MadSlime.Upgrades.Logic;
using MadSlime.Validation;

namespace MadSlime.Game.Logic
{
    public class AdrenalineBoostEffect : IFinalCountdownEffect
    {
        private AdrenalineBoost _adrenalineBoost;

        public AdrenalineBoostEffect(AdrenalineBoost adrenalineBoost)
        {
            ArgumentGuard.CheckNotNull(adrenalineBoost, nameof(adrenalineBoost));
            _adrenalineBoost = adrenalineBoost;
        }

        public void Start()
        {
            _adrenalineBoost.Enable();
        }

        public void Stop()
        {
            _adrenalineBoost.Disable();
        }
    }
}
