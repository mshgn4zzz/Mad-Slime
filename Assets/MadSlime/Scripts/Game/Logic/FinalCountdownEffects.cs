using System.Collections.Generic;
using MadSlime.Validation;

namespace MadSlime.Game.Logic
{
    public class FinalCountdownEffects
    {
        private List<IFinalCountdownEffect> _effects;

        public FinalCountdownEffects(IEnumerable<IFinalCountdownEffect> effects)
        {
            ArgumentGuard.CheckNotNull(effects, nameof(effects));
            _effects = new List<IFinalCountdownEffect>(effects);
        }

        public void Start()
        {
            foreach (IFinalCountdownEffect effect in _effects)
            {
                effect.Start();
            }
        }

        public void SetUrgency(float urgency)
        {
            foreach (IFinalCountdownEffect effect in _effects)
            {
                if (effect is ICountdownTimeReceiver countdownTimeReceiver)
                {
                    countdownTimeReceiver.UpdateProgress(urgency);
                }
            }
        }

        public void Stop()
        {
            foreach (IFinalCountdownEffect effect in _effects)
            {
                effect.Stop();
            }
        }
    }
}
