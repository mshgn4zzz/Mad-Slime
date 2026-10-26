using Audio;
using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
namespace UI.Audio
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

        [Inject]
        public void Construct(AudioMixerController mixerController, IUISoundPlayer soundPlayer)
        {
            _mixerController = mixerController;
            _soundPlayer = soundPlayer;
        }

        private void Awake()
        {
            if (_mixerController == null)
            {
                throw new InvalidOperationException(
                    $"{name}: AudioMixerController was not injected. The settings prefab must be "
                        + $"instantiated through the DI container (IObjectResolver.Instantiate).");
            }

            if (_soundPlayer == null)
            {
                throw new InvalidOperationException(
                    $"{name}: IUISoundPlayer was not injected. The settings prefab must be "
                        + $"instantiated through the DI container (IObjectResolver.Instantiate).");
            }

            if (_musicSlider == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Music Slider is not assigned. Drag a Slider into the _musicSlider field.");
            }

            if (_sfxSlider == null)
            {
                throw new InvalidOperationException(
                    $"{name}: SFX Slider is not assigned. Drag a Slider into the _sfxSlider field.");
            }

            if (_tickClip == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Tick Clip is not assigned. Drag a SfxClip asset into the _tickClip field.");
            }

            _musicSlider.minValue = 0f;
            _sfxSlider.minValue = 0f;

            _musicSlider.maxValue = MaxSliderValue;
            _sfxSlider.maxValue = MaxSliderValue;

            // Slider holds 0..20 whole steps; each step equals VolumeStepPercent percents.
            _musicSlider.wholeNumbers = true;
            _sfxSlider.wholeNumbers = true;
        }

        private void OnEnable()
        {
            _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
            _mixerController.VolumesChanged += RefreshVolumes;
            RefreshVolumes();
        }

        private void OnDisable()
        {
            _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            _sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
            _mixerController.VolumesChanged -= RefreshVolumes;

            _mixerController.CommitVolumes();
        }

        private void RefreshVolumes()
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
    }
}
