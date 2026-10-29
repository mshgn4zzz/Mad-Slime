using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Audio.View
{
    public class FillCompleteSound : MonoBehaviour
    {
        [SerializeField] private SfxClip _sfxClip;

        private IGameSoundPlayer _soundPlayer;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_soundPlayer, name, nameof(_soundPlayer));

            ArgumentGuard.CheckAssigned(_sfxClip, name, nameof(_sfxClip));
        }

        [Inject]
        public void Construct(IGameSoundPlayer soundPlayer)
        {
            _soundPlayer = soundPlayer;
        }

        public void Play()
        {
            _soundPlayer.Play(_sfxClip);
        }
    }
}
