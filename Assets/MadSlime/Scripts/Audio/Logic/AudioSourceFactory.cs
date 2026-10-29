using UnityEngine;
using UnityEngine.Audio;

namespace MadSlime.Audio.Logic
{
    public class AudioSourceFactory
    {
        public AudioSource Create(Transform owner, AudioMixerGroup mixerGroup)
        {
            AudioSource source = owner.gameObject.AddComponent<AudioSource>();

            source.outputAudioMixerGroup = mixerGroup;
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;

            return source;
        }
    }
}
