using System.Collections.Generic;
using UnityEngine;

namespace MadSlime.Audio.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Playlist", fileName = "NewPlaylist")]
    public class Playlist : ScriptableObject
    {
        [SerializeField] private List<AudioClip> _tracks = new List<AudioClip>();

        [UnityEngine.Serialization.FormerlySerializedAs("_randomStart")]
        [SerializeField] private bool _isRandomStart;

        public IReadOnlyList<AudioClip> Tracks => _tracks;

        public bool IsRandomStart => _isRandomStart;
    }
}
