using Saves;
using System;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace Audio
{
    public class AudioMixerController : MonoBehaviour
    {
        private const string MusicVolumeParam = "MusicVolume";
        private const string SfxVolumeParam = "SFXVolume";
        private const float MutedDecibels = -80f;
        private const float MinAudibleDecibels = -20f;
        private const float MuteZone01 = 0.05f;
        private const int MinVolume = 0;
        private const int MaxVolume = 100;
        private const int VolumeStep = 5;

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField] private AudioMixerGroup _sfxGroup;

        private IAudioStorage _audioStorage;
        private ISavesReadiness _savesReadiness;
        private int _musicVolume;
        private int _sfxVolume;
        private bool _isVolumesLoaded;
        private bool _hasUnsavedChanges;
        private bool _hasStarted;

        public event Action VolumesChanged;

        [Inject]
        public void Construct(IAudioStorage audioStorage, ISavesReadiness savesReadiness)
        {
            _audioStorage = audioStorage;
            _savesReadiness = savesReadiness;
        }

        public int MusicVolume => _musicVolume;
        public int SFXVolume => _sfxVolume;
        public bool IsVolumesLoaded => _isVolumesLoaded;

        private void Awake()
        {
            if (_mixer == null)
            {
                throw new InvalidOperationException(
                    $"{name}: AudioMixer is not assigned. Drag MasterMixer into the _mixer field.");
            }

            if (_musicGroup == null)
            {
                throw new InvalidOperationException(
                    $"{name}: Music AudioMixerGroup is not assigned. Drag a group into the _musicGroup field.");
            }

            if (_sfxGroup == null)
            {
                throw new InvalidOperationException(
                    $"{name}: SFX AudioMixerGroup is not assigned. Drag a group into the _sfxGroup field.");
            }

            if (_audioStorage == null || _savesReadiness == null)
            {
                throw new InvalidOperationException(
                    $"{name}: saves dependencies were not injected. Check that ProjectLifetimeScope "
                        + $"registers Saver and AudioMixerController.");
            }
        }

        private void OnEnable()
        {
            _savesReadiness.Ready += OnSavesReady;
            ApplyFromSaves();
        }

        private void OnDisable()
        {
            _savesReadiness.Ready -= OnSavesReady;
        }

        private void Start()
        {
            _hasStarted = true;
            ApplyMusic();
            ApplySFX();
        }

        private void OnSavesReady()
        {
            ApplyFromSaves();
        }

        private void ApplyFromSaves()
        {
            if (_savesReadiness.IsReady == false)
            {
                return;
            }

            _musicVolume = SnapToStep(_audioStorage.MusicVolume);
            _sfxVolume = SnapToStep(_audioStorage.SfxVolume);
            _isVolumesLoaded = true;
            _hasUnsavedChanges = false;

            ApplyMusic();
            ApplySFX();
            VolumesChanged?.Invoke();
        }

        public void SetMusicVolume(int volume)
        {
            _musicVolume = Mathf.Clamp(volume, MinVolume, MaxVolume);
            _hasUnsavedChanges = true;

            ApplyMusic();
            VolumesChanged?.Invoke();
        }

        public void SetSFXVolume(int volume)
        {
            _sfxVolume = Mathf.Clamp(volume, MinVolume, MaxVolume);
            _hasUnsavedChanges = true;

            ApplySFX();
            VolumesChanged?.Invoke();
        }

        public void CommitVolumes()
        {
            if (_isVolumesLoaded == false || _hasUnsavedChanges == false)
            {
                return;
            }

            _hasUnsavedChanges = false;
            _audioStorage.SetVolumes(_musicVolume, _sfxVolume);
        }

        private void ApplyMusic()
        {
            if (_hasStarted == false || _isVolumesLoaded == false)
            {
                return;
            }

            float decibels = LinearToDecibels(_musicVolume / (float)MaxVolume);
            _mixer.SetFloat(MusicVolumeParam, decibels);
        }

        private void ApplySFX()
        {
            if (_hasStarted == false || _isVolumesLoaded == false)
            {
                return;
            }

            float decibels = LinearToDecibels(_sfxVolume / (float)MaxVolume);
            _mixer.SetFloat(SfxVolumeParam, decibels);
        }

        private static int SnapToStep(int volume)
        {
            int stepped = Mathf.RoundToInt(volume / (float)VolumeStep) * VolumeStep;
            return Mathf.Clamp(stepped, MinVolume, MaxVolume);
        }

        // Slider is linear in dB (equal steps = equal perceived loudness change):
        // right edge = 0 dB, below MuteZone01 the curve dives smoothly into silence.
        private static float LinearToDecibels(float volume01)
        {
            if (volume01 <= Mathf.Epsilon)
            {
                return MutedDecibels;
            }

            if (volume01 <= MuteZone01)
            {
                float muteProgress = volume01 / MuteZone01;
                return Mathf.Lerp(MutedDecibels, MinAudibleDecibels, muteProgress);
            }

            float audibleProgress = (volume01 - MuteZone01) / (1f - MuteZone01);
            return Mathf.Lerp(MinAudibleDecibels, 0f, audibleProgress);
        }
    }
}
