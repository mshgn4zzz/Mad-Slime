using System;
using UnityEngine;
namespace Levels
{
    [Serializable]
    public class LevelRange
    {
        [SerializeField] private int _fromLevel = 1;

        [SerializeField] private int _toLevel = 3;

        [SerializeField] private LevelConfig _config;

        public int FromLevel => _fromLevel;
        public int ToLevel => _toLevel;
        public LevelConfig Config => _config;
    }
}
