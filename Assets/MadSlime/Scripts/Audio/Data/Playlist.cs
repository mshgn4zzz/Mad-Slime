using System.Collections.Generic;
using UnityEngine;
namespace Audio
{
    [CreateAssetMenu(menuName = "Mad Slime/Playlist", fileName = "NewPlaylist")]
    public class Playlist : ScriptableObject
    {
        [Tooltip("Треки плейлиста сцены: играют по порядку с плавными переходами, последний переходит в первый.")]
        [SerializeField] private List<AudioClip> _tracks = new List<AudioClip>();

        [Tooltip("При входе в плейлист начинать со случайного трека, затем продолжать по порядку.")]
        [SerializeField] private bool _randomStart;

        public IReadOnlyList<AudioClip> Tracks => _tracks;
        public bool RandomStart => _randomStart;
    }
}
