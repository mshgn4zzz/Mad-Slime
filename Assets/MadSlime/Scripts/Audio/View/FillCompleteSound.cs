using System;
using UnityEngine;
using VContainer;
namespace Audio
{
    public class FillCompleteSound : MonoBehaviour
    {
        [SerializeField] private SfxClip _sfxClip;

        private IGameSoundPlayer _soundPlayer;

        [Inject]
        public void Construct(IGameSoundPlayer soundPlayer)
        {
            _soundPlayer = soundPlayer;
        }

        private void Awake()
        {
            if (_soundPlayer == null)
            {
                throw new InvalidOperationException(
                    $"{name}: IGameSoundPlayer was not injected. Check that FillLifetimeScope "
                        + $"registers FillCompleteSound.");
            }

            if (_sfxClip == null)
            {
                throw new InvalidOperationException(
                    $"{name}: SfxClip is not assigned. Drag a clap SfxClip asset into the _sfxClip field.");
            }
        }

        public void Play()
        {
            _soundPlayer.Play(_sfxClip);
        }
    }
}
