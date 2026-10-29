using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.Audio.Logic;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MadSlime.UI.Audio
{
    public class AudioSettingsPanel : MonoBehaviour
    {
        private const float TickThrottleSeconds = 0.08f;
        private const int VolumeStepPercent = 5;
        private const float MaxSliderValue = 100f / VolumeStepPercent;

        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private SfxClip _tickClip;

        private AudioMixerController _mixerController;
        private IUISoundPlayer _soundPlayer;
        private float _lastTickTime;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_mixerController, name, nameof(_mixerController));

            ArgumentGuard.CheckAssigned(_soundPlayer, name, nameof(_soundPlayer));

            ArgumentGuard.CheckAssigned(_musicSlider, name, nameof(_musicSlider));

            ArgumentGuard.CheckAssigned(_sfxSlider, name, nameof(_sfxSlider));

            ArgumentGuard.CheckAssigned(_tickClip, name, nameof(_tickClip));

            _musicSlider.minValue = 0f;
            _sfxSlider.minValue = 0f;

            _musicSlider.maxValue = MaxSliderValue;
            _sfxSlider.maxValue = MaxSliderValue;

            _musicSlider.wholeNumbers = true;
            _sfxSlider.wholeNumbers = true;
        }

        private void OnEnable()
        {
            _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
            _mixerController.VolumesChanged += OnVolumesChanged;
            OnVolumesChanged();
        }

        private void OnDisable()
        {
            _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
            _mixerController.VolumesChanged -= OnVolumesChanged;

            _mixerController.CommitVolumes();
        }

        [Inject]
        public void Construct(AudioMixerController mixerController, IUISoundPlayer soundPlayer)
        {
            _mixerController = mixerController;
            _soundPlayer = soundPlayer;
        }

        private void PlayTick()
        {
            float currentTime = Time.unscaledTime;

            if (currentTime - _lastTickTime < TickThrottleSeconds)
            {
                return;
            }

            _lastTickTime = currentTime;
            _soundPlayer.Play(_tickClip);
        }

        private void OnVolumesChanged()
        {
            _musicSlider.SetValueWithoutNotify(_mixerController.MusicVolume / (float)VolumeStepPercent);
            _sfxSlider.SetValueWithoutNotify(_mixerController.SfxVolume / (float)VolumeStepPercent);
            _musicSlider.interactable = _mixerController.IsVolumesLoaded;
            _sfxSlider.interactable = _mixerController.IsVolumesLoaded;
        }

        private void OnMusicSliderChanged(float value)
        {
            _mixerController.SetMusicVolume((int)value * VolumeStepPercent);
        }

        private void OnSfxSliderChanged(float value)
        {
            _mixerController.SetSfxVolume((int)value * VolumeStepPercent);
            PlayTick();
        }
    }
}
