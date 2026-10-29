using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.Collectables.Logic;
using MadSlime.Player.Data;
using MadSlime.Validation;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace MadSlime.Audio.View
{
    public class PlayerPickupSound : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private SfxClip _sfxClip;

        private IGameSoundPlayer _soundPlayer;
        private ItemCollector _itemCollector;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_soundPlayer, name, nameof(_soundPlayer));

            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));

            ArgumentGuard.CheckAssigned(_itemCollector, name, nameof(_itemCollector));

            ArgumentGuard.CheckAssigned(_sfxClip, name, nameof(_sfxClip));

            ArgumentGuard.CheckOrder(
                _config.PickupSoundMinInterval,
                _config.PickupSoundMaxInterval,
                name,
                nameof(_config.PickupSoundMinInterval));

            ArgumentGuard.CheckOrder(
                _config.PickupSoundMinPitch, _config.PickupSoundMaxPitch, name, nameof(_config.PickupSoundMinPitch));
        }

        private void OnEnable()
        {
            _itemCollector.ItemCollected += OnItemCollected;
        }

        private void OnDisable()
        {
            _itemCollector.ItemCollected -= OnItemCollected;
        }

        [Inject]
        public void Construct(IGameSoundPlayer soundPlayer, ItemCollector itemCollector)
        {
            _soundPlayer = soundPlayer;
            _itemCollector = itemCollector;
        }

        private void OnItemCollected(global::MadSlime.Items.View.Item item)
        {
            float throttleSeconds = Random.Range(
                _config.PickupSoundMinInterval,
                _config.PickupSoundMaxInterval);

            _soundPlayer.Play(_sfxClip, throttleSeconds);
        }
    }
}
