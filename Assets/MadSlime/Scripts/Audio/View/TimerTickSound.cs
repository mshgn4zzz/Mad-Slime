using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Audio.View
{
    public class TimerTickSound : MonoBehaviour
    {
        [SerializeField] private SfxClip _sfxClip;

        private IGameSoundPlayer _soundPlayer;
        private bool _isTickingActive;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_soundPlayer, name, nameof(_soundPlayer));

            ArgumentGuard.CheckAssigned(_sfxClip, name, nameof(_sfxClip));
        }

        private void OnDisable()
        {
            StopTicking();
        }

        [Inject]
        public void Construct(IGameSoundPlayer soundPlayer)
        {
            _soundPlayer = soundPlayer;
        }

        public void StartTicking()
        {
            if (_isTickingActive)
            {
                return;
            }

            _soundPlayer.StartLoop(_sfxClip);
            _isTickingActive = true;
        }

        public void StopTicking()
        {
            if (_isTickingActive == false)
            {
                return;
            }

            _soundPlayer.StopLoop();
            _isTickingActive = false;
        }
    }
}
