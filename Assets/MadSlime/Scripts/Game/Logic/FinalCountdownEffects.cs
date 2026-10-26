using Audio;
using Player;
using System;
using Upgrades;
namespace Game
{
    public class FinalCountdownEffects
    {
        private AdrenalineBoost _adrenalineBoost;
        private FinalCountdownVignette _vignette;
        private TimerTickSound _tickSound;
        private CrawlAnimator _crawlAnimator;

        public FinalCountdownEffects(AdrenalineBoost adrenalineBoost, FinalCountdownVignette vignette,
            TimerTickSound tickSound, CrawlAnimator crawlAnimator)
        {
            ArgumentGuard.CheckNotNull(adrenalineBoost, nameof(adrenalineBoost));

            ArgumentGuard.CheckNotNull(vignette, nameof(vignette));

            ArgumentGuard.CheckNotNull(tickSound, nameof(tickSound));

            ArgumentGuard.CheckNotNull(crawlAnimator, nameof(crawlAnimator));

            _adrenalineBoost = adrenalineBoost;
            _vignette = vignette;
            _tickSound = tickSound;
            _crawlAnimator = crawlAnimator;
        }

        public void SetUrgency(float urgency)
        {
            _crawlAnimator.SetUrgency(urgency);
        }

        public void Start()
        {
            _adrenalineBoost.Enable();
            _vignette.StartPulse();
            _tickSound.StartTicking();
        }

        public void Stop()
        {
            _crawlAnimator.SetUrgency(0f);
            _adrenalineBoost.Disable();
            _vignette.StopPulse();
            _tickSound.StopTicking();
        }
    }
}
