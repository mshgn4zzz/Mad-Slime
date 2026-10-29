using MadSlime.Levels.Data;
using MadSlime.Validation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MadSlime.Levels.Logic
{
    public class LevelEnvironment
    {
        private Transform _parent;
        private LevelEnvironmentFactory _factory;
        private EnvironmentRoot _instance;
        private EnvironmentRoot _prefab;

        public LevelEnvironment(Transform parent, LevelEnvironmentFactory factory)
        {
            ArgumentGuard.CheckNotNull(parent, nameof(parent));

            _parent = parent;
            _factory = factory;
        }

        public void Apply(LevelTheme theme, Bounds floorBounds)
        {
            ArgumentGuard.CheckNotNull(theme, nameof(theme));

            if (_prefab != theme.EnvironmentPrefab)
            {
                Clear();
            }

            if (theme.EnvironmentPrefab == null)
            {
                return;
            }

            Vector3 position = floorBounds.center;

            position.y = floorBounds.max.y;

            if (_instance == null)
            {
                _instance = _factory.Create(theme.EnvironmentPrefab, position, _parent);
                _prefab = theme.EnvironmentPrefab;
            }
            else
            {
                _instance.transform.position = position;
            }
        }

        public void Clear()
        {
            if (_instance != null)
            {
                _instance.gameObject.SetActive(false);
                Object.Destroy(_instance.gameObject);
            }

            _instance = null;
            _prefab = null;
        }
    }
}
