using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(menuName = "Mad Slime/Sfx Clip", fileName = "NewSfxClip")]
    public class SfxClip : ScriptableObject
    {
        [SerializeField] private AudioClip _clip;

        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        [SerializeField] private bool _isRandomPitch;

        [SerializeField, Range(0.1f, 3f)] private float _minPitch = 1f;

        [SerializeField, Range(0.1f, 3f)] private float _maxPitch = 1f;

        public AudioClip Clip => _clip;

        public float Volume => _volume;

        public bool IsRandomPitch => _isRandomPitch;

        public float MinPitch => _minPitch;

        public float MaxPitch => _maxPitch;
    }
}