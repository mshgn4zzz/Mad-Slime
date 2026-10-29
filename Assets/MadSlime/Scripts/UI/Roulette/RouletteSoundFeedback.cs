using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.Roulette.Data;
using UnityEngine;

namespace MadSlime.UI.Roulette
{
    public class RouletteSoundFeedback
    {
        private const float MinTickIntervalSeconds = 0.06f;

        private RouletteWheel _wheel;
        private RouletteConfig _config;
        private IUISoundPlayer _soundPlayer;
        private float _lastTickTime;

        public void Setup(RouletteWheel wheel, RouletteConfig config, IUISoundPlayer soundPlayer)
        {
            _wheel = wheel;
            _config = config;
            _soundPlayer = soundPlayer;
            _wheel.Ticked += OnTick;
        }

        public void Release()
        {
            if (_wheel != null)
            {
                _wheel.Ticked -= OnTick;
            }
        }

        public void PlaySpin()
        {
            Play(_config.SpinStartClip);
        }

        public void PlayWin()
        {
            Play(_config.WinClip);
        }

        private void Play(SfxClip clip)
        {
            if (clip != null)
            {
                _soundPlayer.Play(clip);
            }
        }

        private void OnTick()
        {
            if (Time.unscaledTime - _lastTickTime < MinTickIntervalSeconds)
            {
                return;
            }

            _lastTickTime = Time.unscaledTime;
            Play(_config.StepClip);
        }
    }
}
