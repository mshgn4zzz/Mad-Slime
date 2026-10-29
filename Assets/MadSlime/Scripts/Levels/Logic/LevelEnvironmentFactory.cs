using MadSlime.Levels.Data;
using UnityEngine;

namespace MadSlime.Levels.Logic
{
    public class LevelEnvironmentFactory
    {
        public EnvironmentRoot Create(EnvironmentRoot prefab, Vector3 position, Transform parent)
        {
            return Object.Instantiate(prefab, position, Quaternion.identity, parent);
        }
    }
}
