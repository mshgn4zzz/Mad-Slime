using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MadSlime.Audio.Logic
{
    internal class AudioVoicePool
    {
        private const int MaxSourceCount = 24;

        private List<AudioSource> _sources = new List<AudioSource>();
        private List<float> _sourceEndTimes = new List<float>();
        private AudioSourceFactory _sourceFactory = new AudioSourceFactory();
        private AudioMixerGroup _mixerGroup;
        private Transform _ownerTransform;

        public AudioVoicePool(int sourceCount, AudioMixerGroup mixerGroup, Transform ownerTransform)
        {
            _mixerGroup = mixerGroup;
            _ownerTransform = ownerTransform;

            for (int i = 0; i < sourceCount; i++)
            {
                AddSource();
            }
        }

        public void Play(AudioClip clip, float volume, float pitch)
        {
            int freeIndex = FindFreeSourceIndex();

            if (freeIndex < 0)
            {
                if (_sources.Count < MaxSourceCount)
                {
                    freeIndex = AddSource();
                }
                else
                {
                    freeIndex = FindSoonestSourceIndex();
                }
            }

            AudioSource source = _sources[freeIndex];

            source.pitch = pitch;
            source.volume = volume;
            source.PlayOneShot(clip);
            _sourceEndTimes[freeIndex] = Time.unscaledTime + (clip.length / pitch);
        }

        private int AddSource()
        {
            AudioSource source = _sourceFactory.Create(_ownerTransform, _mixerGroup);

            _sources.Add(source);
            _sourceEndTimes.Add(0f);

            return _sources.Count - 1;
        }

        private int FindFreeSourceIndex()
        {
            for (int index = 0; index < _sources.Count; index++)
            {
                if (Time.unscaledTime >= _sourceEndTimes[index])
                {
                    return index;
                }
            }

            return -1;
        }

        private int FindSoonestSourceIndex()
        {
            int soonestIndex = 0;

            for (int index = 1; index < _sourceEndTimes.Count; index++)
            {
                if (_sourceEndTimes[index] < _sourceEndTimes[soonestIndex])
                {
                    soonestIndex = index;
                }
            }

            return soonestIndex;
        }
    }
}
