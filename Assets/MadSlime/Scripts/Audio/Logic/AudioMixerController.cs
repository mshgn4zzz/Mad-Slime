using System;
using MadSlime.Saves.Interfaces;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace MadSlime.Audio.Logic
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
        private bool _isSavePending;
        private bool _isStarted;

        public event Action VolumesChanged;

        public int MusicVolume => _musicVolume;

        public int SfxVolume => _sfxVolume;

        public bool IsVolumesLoaded => _isVolumesLoaded;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_mixer, name, nameof(_mixer));
            ArgumentGuard.CheckAssigned(_musicGroup, name, nameof(_musicGroup));
            ArgumentGuard.CheckAssigned(_sfxGroup, name, nameof(_sfxGroup));
            ArgumentGuard.CheckAssigned(_audioStorage, name, nameof(_audioStorage));
            ArgumentGuard.CheckAssigned(_savesReadiness, name, nameof(_savesReadiness));
        }

        private void OnEnable()
        {
            _savesReadiness.Prepared += OnSavesReady;
            ApplyFromSaves();
        }

        private void OnDisable()
        {
            _savesReadiness.Prepared -= OnSavesReady;
        }

        private void Start()
        {
            _isStarted = true;
            ApplyChannelVolume(MusicVolumeParam, _musicVolume);
            ApplyChannelVolume(SfxVolumeParam, _sfxVolume);
        }

        [Inject]
        public void Construct(IAudioStorage audioStorage, ISavesReadiness savesReadiness)
        {
            _audioStorage = audioStorage;
            _savesReadiness = savesReadiness;
        }

        public void SetMusicVolume(int volume)
        {
            _musicVolume = ClampVolume(volume);
            _isSavePending = true;

            ApplyChannelVolume(MusicVolumeParam, _musicVolume);
            VolumesChanged?.Invoke();
        }

        public void SetSfxVolume(int volume)
        {
            _sfxVolume = ClampVolume(volume);
            _isSavePending = true;

            ApplyChannelVolume(SfxVolumeParam, _sfxVolume);
            VolumesChanged?.Invoke();
        }

        public void CommitVolumes()
        {
            if (_isVolumesLoaded == false || _isSavePending == false)
            {
                return;
            }

            _isSavePending = false;
            _audioStorage.SetVolumes(_musicVolume, _sfxVolume);
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
            _isSavePending = false;

            ApplyChannelVolume(MusicVolumeParam, _musicVolume);
            ApplyChannelVolume(SfxVolumeParam, _sfxVolume);
            VolumesChanged?.Invoke();
        }

        private int ClampVolume(int volume)
        {
            return Mathf.Clamp(volume, MinVolume, MaxVolume);
        }

        private void ApplyChannelVolume(string mixerParameter, int volume)
        {
            if (_isStarted == false || _isVolumesLoaded == false)
            {
                return;
            }

            float decibels = LinearToDecibels(volume / (float)MaxVolume);

            _mixer.SetFloat(mixerParameter, decibels);
        }

        private int SnapToStep(int volume)
        {
            int stepped = Mathf.RoundToInt(volume / (float)VolumeStep) * VolumeStep;

            return Mathf.Clamp(stepped, MinVolume, MaxVolume);
        }

        private float LinearToDecibels(float volume01)
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

        private void OnSavesReady()
        {
            ApplyFromSaves();
        }
    }
}
