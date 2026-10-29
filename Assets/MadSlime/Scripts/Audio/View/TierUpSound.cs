using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.Items.Data;
using MadSlime.Player.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Audio.View
{
    public class TierUpSound : MonoBehaviour
    {
        [SerializeField] private SfxClip _sfxClip;

        private IGameSoundPlayer _soundPlayer;
        private PlayerTier _playerTier;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_soundPlayer, name, nameof(_soundPlayer));

            ArgumentGuard.CheckAssigned(_playerTier, name, nameof(_playerTier));

            ArgumentGuard.CheckAssigned(_sfxClip, name, nameof(_sfxClip));
        }

        private void OnEnable()
        {
            _playerTier.TierChanged += OnTierChanged;
        }

        private void OnDisable()
        {
            _playerTier.TierChanged -= OnTierChanged;
        }

        [Inject]
        public void Construct(IGameSoundPlayer soundPlayer, PlayerTier playerTier)
        {
            _soundPlayer = soundPlayer;
            _playerTier = playerTier;
        }

        private void OnTierChanged(SizeTier previousTier, SizeTier currentTier)
        {
            if (currentTier <= previousTier)
            {
                return;
            }

            _soundPlayer.Play(_sfxClip);
        }
    }
}
