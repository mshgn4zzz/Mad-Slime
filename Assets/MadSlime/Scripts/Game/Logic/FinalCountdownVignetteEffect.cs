using MadSlime.Player.View;
using MadSlime.Validation;

namespace MadSlime.Game.Logic
{
    public class FinalCountdownVignetteEffect : IFinalCountdownEffect
    {
        private FinalCountdownVignette _vignette;

        public FinalCountdownVignetteEffect(FinalCountdownVignette vignette)
        {
            ArgumentGuard.CheckNotNull(vignette, nameof(vignette));
            _vignette = vignette;
        }

        public void Start()
        {
            _vignette.StartPulse();
        }

        public void Stop()
        {
            _vignette.StopPulse();
        }
    }
}
