using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.Upgrades.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Audio.View
{
    public class AdrenalineSound : MonoBehaviour
    {
        [SerializeField] private SfxClip _sfxClip;

        private IGameSoundPlayer _soundPlayer;
        private AdrenalineBoost _adrenalineBoost;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_soundPlayer, name, nameof(_soundPlayer));

            ArgumentGuard.CheckAssigned(_adrenalineBoost, name, nameof(_adrenalineBoost));
        }

        private void OnEnable()
        {
            _adrenalineBoost.BoostStarted += OnBoostStarted;
        }

        private void OnDisable()
        {
            _adrenalineBoost.BoostStarted -= OnBoostStarted;
        }

        [Inject]
        public void Construct(IGameSoundPlayer soundPlayer, AdrenalineBoost adrenalineBoost)
        {
            _soundPlayer = soundPlayer;
            _adrenalineBoost = adrenalineBoost;
        }

        private void OnBoostStarted()
        {
            if (_sfxClip == null)
            {
                return;
            }

            _soundPlayer.Play(_sfxClip);
        }
    }
}
