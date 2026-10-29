using System.Collections.Generic;
using MadSlime.Audio.Data;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Audio.Logic
{
    public class MusicChannel
    {
        private const int SourceCount = 2;

        private AudioSource[] _sources = new AudioSource[SourceCount];
        private bool[] _isSourceFadingOut = new bool[SourceCount];
        private float _fadeInDuration;
        private float _fadeOutDuration;
        private float _overlapDuration;

        private IReadOnlyList<AudioClip> _tracks;
        private int _trackIndex;
        private int _activeIndex;
        private bool _isLeadFadingIn;

        public MusicChannel(
            AudioSource firstSource,
            AudioSource secondSource,
            float fadeInDuration,
            float fadeOutDuration,
            float overlapDuration)
        {
            ArgumentGuard.CheckNotNull(firstSource, nameof(firstSource));

            ArgumentGuard.CheckNotNull(secondSource, nameof(secondSource));

            ArgumentGuard.CheckPositive(fadeInDuration, nameof(fadeInDuration));

            ArgumentGuard.CheckPositive(fadeOutDuration, nameof(fadeOutDuration));

            ArgumentGuard.CheckPositive(overlapDuration, nameof(overlapDuration));

            _sources[0] = firstSource;
            _sources[1] = secondSource;
            _fadeInDuration = fadeInDuration;
            _fadeOutDuration = fadeOutDuration;
            _overlapDuration = overlapDuration;
        }

        public void Play(Playlist playlist)
        {
            ArgumentGuard.CheckNotNull(playlist, nameof(playlist));

            ArgumentGuard.CheckNotEmpty(playlist.Tracks, playlist.name, nameof(playlist.Tracks));

            if (IsSamePlaylist(playlist) && _sources[_activeIndex].clip != null)
            {
                return;
            }

            _tracks = playlist.Tracks;
            _trackIndex = 0;

            if (playlist.IsRandomStart)
            {
                _trackIndex = UnityEngine.Random.Range(0, _tracks.Count);
            }

            AudioSource leadSource = _sources[_activeIndex];

            if (leadSource.clip != null)
            {
                _isSourceFadingOut[_activeIndex] = true;
            }

            _activeIndex = 1 - _activeIndex;
            StartLeadTrack(_tracks[_trackIndex]);
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (_tracks == null || _tracks.Count == 0)
            {
                return;
            }

            AdvanceLeadFadeIn(unscaledDeltaTime);
            AdvanceFadeOuts(unscaledDeltaTime);
            ScheduleCrossfadeBeforeTrackEnd();
        }

        private void StartLeadTrack(AudioClip track)
        {
            AudioSource leadSource = _sources[_activeIndex];

            leadSource.Stop();
            leadSource.clip = track;
            leadSource.volume = 0f;
            leadSource.Play();

            _isLeadFadingIn = true;
            _isSourceFadingOut[_activeIndex] = false;
        }

        private void AdvanceLeadFadeIn(float unscaledDeltaTime)
        {
            if (_isLeadFadingIn == false)
            {
                return;
            }

            AudioSource leadSource = _sources[_activeIndex];
            float fadeStep = unscaledDeltaTime / _fadeInDuration;
            float newVolume = Mathf.Min(1f, leadSource.volume + fadeStep);

            leadSource.volume = newVolume;

            if (newVolume >= 1f)
            {
                _isLeadFadingIn = false;
            }
        }

        private void AdvanceFadeOuts(float unscaledDeltaTime)
        {
            for (int sourceIndex = 0; sourceIndex < SourceCount; sourceIndex++)
            {
                if (_isSourceFadingOut[sourceIndex] == false)
                {
                    continue;
                }

                AudioSource source = _sources[sourceIndex];
                float fadeStep = unscaledDeltaTime / _fadeOutDuration;
                float newVolume = Mathf.Max(0f, source.volume - fadeStep);

                source.volume = newVolume;

                if (newVolume <= 0f)
                {
                    source.Stop();
                    source.clip = null;
                    _isSourceFadingOut[sourceIndex] = false;
                }
            }
        }

        private void ScheduleCrossfadeBeforeTrackEnd()
        {
            AudioSource leadSource = _sources[_activeIndex];

            if (leadSource.clip == null || AudioListener.pause)
            {
                return;
            }

            int outgoingIndex = 1 - _activeIndex;

            if (_isLeadFadingIn || _isSourceFadingOut[outgoingIndex])
            {
                return;
            }

            float remaining = 0f;

            if (leadSource.isPlaying)
            {
                remaining = leadSource.clip.length - leadSource.time;
            }

            if (remaining > _overlapDuration)
            {
                return;
            }

            _trackIndex = (_trackIndex + 1) % _tracks.Count;

            int incomingIndex = 1 - _activeIndex;

            _isSourceFadingOut[_activeIndex] = true;
            _activeIndex = incomingIndex;
            StartLeadTrack(_tracks[_trackIndex]);
        }

        private bool IsSamePlaylist(Playlist playlist)
        {
            if (_tracks == null || _tracks.Count != playlist.Tracks.Count)
            {
                return false;
            }

            for (int i = 0; i < _tracks.Count; i++)
            {
                if (_tracks[i] != playlist.Tracks[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
