using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(menuName = "Mad Slime/Playlist", fileName = "NewPlaylist")]
    public class Playlist : ScriptableObject
    {
        [SerializeField] private List<AudioClip> _tracks = new List<AudioClip>();

        [SerializeField] private bool _randomStart;

        public IReadOnlyList<AudioClip> Tracks => _tracks;

        public bool RandomStart => _randomStart;
    }
}
