using System.Collections.Generic;
using MadSlime.Audio.Data;
using MadSlime.Audio.Interfaces;
using MadSlime.Validation;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace MadSlime.Audio.Logic
{
    public class AudioPlayer : MonoBehaviour, IMusicPlayer, IGameSoundPlayer, IUISoundPlayer
    {
        [SerializeField] private AudioMixerGroup _sfxGroup;

        [SerializeField] private AudioMixerGroup _musicGroup;

        [SerializeField, Min(1)] private int _uiVoices = 2;

        [SerializeField, Min(1)] private int _gameVoices = 5;

        [SerializeField, Min(0.01f)] private float _musicFadeIn = 1f;

        [SerializeField, Min(0.01f)] private float _musicFadeOut = 1.5f;

        [SerializeField, Min(0.01f)] private float _musicOverlapDuration = 2.5f;

        private AudioSourceFactory _sourceFactory = new AudioSourceFactory();
        private AudioVoicePool _uiPool;
        private AudioVoicePool _gamePool;
        private AudioSource _loopSource;
        private MusicChannel _musicChannel;
        private Dictionary<int, float> _nextAllowedPlayTimeByClipId = new Dictionary<int, float>();

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_sfxGroup, name, nameof(_sfxGroup));

            ArgumentGuard.CheckAssigned(_musicGroup, name, nameof(_musicGroup));

            _uiPool = new AudioVoicePool(_uiVoices, _sfxGroup, transform);
            _gamePool = new AudioVoicePool(_gameVoices, _sfxGroup, transform);

            _loopSource = _sourceFactory.Create(transform, _sfxGroup);
            _loopSource.loop = true;

            AudioSource firstMusicSource = _sourceFactory.Create(transform, _musicGroup);
            AudioSource secondMusicSource = _sourceFactory.Create(transform, _musicGroup);

            _musicChannel = new MusicChannel(
                firstMusicSource,
                secondMusicSource,
                _musicFadeIn,
                _musicFadeOut,
                _musicOverlapDuration);
        }

        private void Update()
        {
            _musicChannel.Tick(Time.unscaledDeltaTime);
        }

        public void Play(Playlist playlist)
        {
            _musicChannel.Play(playlist);
        }

        public void Play(SfxClip clip)
        {
            ValidateSfxClip(clip);
            _uiPool.Play(clip.Clip, clip.Volume, ResolvePitch(clip));
        }

        public void Play(SfxClip clip, float throttleSeconds)
        {
            if (TryPassThrottle(clip, throttleSeconds) == false)
            {
                return;
            }

            ValidateSfxClip(clip);
            _gamePool.Play(clip.Clip, clip.Volume, ResolvePitch(clip));
        }

        public void Play(SfxClip clip, float pitch, float throttleSeconds)
        {
            if (TryPassThrottle(clip, throttleSeconds) == false)
            {
                return;
            }

            ValidateSfxClip(clip);
            _gamePool.Play(clip.Clip, clip.Volume, pitch);
        }

        public void StartLoop(SfxClip clip)
        {
            ValidateSfxClip(clip);

            _loopSource.clip = clip.Clip;
            _loopSource.volume = clip.Volume;
            _loopSource.pitch = 1f;
            _loopSource.Play();
        }

        public void StopLoop()
        {
            if (_loopSource.isPlaying)
            {
                _loopSource.Stop();
            }
        }

        private bool TryPassThrottle(SfxClip clip, float throttleSeconds)
        {
            int clipId = clip.GetInstanceID();
            float currentTime = Time.unscaledTime;

            if (_nextAllowedPlayTimeByClipId.TryGetValue(clipId, out float nextAllowedTime))
            {
                if (currentTime < nextAllowedTime)
                {
                    return false;
                }
            }

            _nextAllowedPlayTimeByClipId[clipId] = currentTime + throttleSeconds;

            return true;
        }

        private float ResolvePitch(SfxClip clip)
        {
            if (clip.IsRandomPitch == false)
            {
                return 1f;
            }

            return Random.Range(clip.MinPitch, clip.MaxPitch);
        }

        private void ValidateSfxClip(SfxClip clip)
        {
            ArgumentGuard.CheckNotNull(clip, nameof(clip));

            ArgumentGuard.CheckAssigned(clip.Clip, clip.name, nameof(clip.Clip));

            if (clip.IsRandomPitch)
            {
                ArgumentGuard.CheckOrder(clip.MinPitch, clip.MaxPitch, clip.name, nameof(clip.MinPitch));
            }
        }
    }
}
