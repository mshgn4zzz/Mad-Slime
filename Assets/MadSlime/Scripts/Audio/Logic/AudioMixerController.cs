using System;
using Saves;
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

        public int SfxVolume => _sfxVolume;

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
                        + $"registers AudioStorage and AudioMixerController.");
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
            ApplyChannelVolume(MusicVolumeParam, _musicVolume);
            ApplyChannelVolume(SfxVolumeParam, _sfxVolume);
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

            int savedMusicVolume = _audioStorage.GetMusicVolume();
            int savedSfxVolume = _audioStorage.GetSfxVolume();

            _musicVolume = SnapToStep(savedMusicVolume);
            _sfxVolume = SnapToStep(savedSfxVolume);

            if (_musicVolume != savedMusicVolume || _sfxVolume != savedSfxVolume)
            {
                _audioStorage.SetVolumes(_musicVolume, _sfxVolume);
            }

            _isVolumesLoaded = true;
            _hasUnsavedChanges = false;

            ApplyChannelVolume(MusicVolumeParam, _musicVolume);
            ApplyChannelVolume(SfxVolumeParam, _sfxVolume);
            VolumesChanged?.Invoke();
        }

        public void SetMusicVolume(int volume)
        {
            _musicVolume = ClampVolume(volume);
            _hasUnsavedChanges = true;

            ApplyChannelVolume(MusicVolumeParam, _musicVolume);
            VolumesChanged?.Invoke();
        }

        public void SetSfxVolume(int volume)
        {
            _sfxVolume = ClampVolume(volume);
            _hasUnsavedChanges = true;

            ApplyChannelVolume(SfxVolumeParam, _sfxVolume);
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

        private static int ClampVolume(int volume)
        {
            return Mathf.Clamp(volume, MinVolume, MaxVolume);
        }

        private void ApplyChannelVolume(string mixerParameter, int volume)
        {
            if (_hasStarted == false || _isVolumesLoaded == false)
            {
                return;
            }

            float decibels = LinearToDecibels(volume / (float)MaxVolume);

            _mixer.SetFloat(mixerParameter, decibels);
        }

        private static int SnapToStep(int volume)
        {
            int stepped = Mathf.RoundToInt(volume / (float)VolumeStep) * VolumeStep;

            return Mathf.Clamp(stepped, MinVolume, MaxVolume);
        }

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
