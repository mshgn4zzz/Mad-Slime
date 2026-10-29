using MadSlime.Audio.View;
using MadSlime.Validation;

namespace MadSlime.Game.Logic
{
    public class TimerTickSoundEffect : IFinalCountdownEffect
    {
        private TimerTickSound _tickSound;

        public TimerTickSoundEffect(TimerTickSound tickSound)
        {
            ArgumentGuard.CheckNotNull(tickSound, nameof(tickSound));
            _tickSound = tickSound;
        }

        public void Start()
        {
            _tickSound.StartTicking();
        }

        public void Stop()
        {
            _tickSound.StopTicking();
        }
    }
}
