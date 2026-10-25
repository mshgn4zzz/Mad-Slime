using System;
using Scriptables;

namespace Game
{
    public class LevelConfigResolver
    {
        private LevelsCatalog _catalog;

        public LevelConfigResolver(LevelsCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog),
                    "LevelConfigResolver requires a LevelsCatalog asset.");
            }

            _catalog = catalog;
        }

        public LevelConfig GetConfigFor(int levelNumber)
        {
            if (_catalog.Ranges.Count == 0)
            {
                throw new InvalidOperationException(
                    $"LevelsCatalog '{_catalog.name}' has no ranges assigned. Add at least one LevelRange.");
            }

            LevelConfig matched = FindRangeConfig(levelNumber);

            if (IsPlayable(matched) == false)
            {
                throw new InvalidOperationException(
                    $"LevelsCatalog '{_catalog.name}': level {levelNumber} resolved to a config that is not playable — " +
                    $"{DescribeProblem(matched)}. Fix the config of this range and run Mad Slime → Prop Factory if needed.");
            }

            return matched;
        }

        private LevelConfig FindRangeConfig(int levelNumber)
        {
            if (levelNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelNumber));
            }

            int cycleLength = _catalog.Ranges[_catalog.Ranges.Count - 1].ToLevel;
            int cycleLevel = (levelNumber - 1) % cycleLength + 1;

            for (int i = 0; i < _catalog.Ranges.Count; i++)
            {
                if (cycleLevel >= _catalog.Ranges[i].FromLevel && cycleLevel <= _catalog.Ranges[i].ToLevel)
                {
                    return _catalog.Ranges[i].Config;
                }
            }

            return _catalog.Ranges[_catalog.Ranges.Count - 1].Config;
        }

        private static bool IsPlayable(LevelConfig config)
        {
            return config != null && config.PropSet != null && config.PropSet.Variants.Count > 0;
        }

        private static string DescribeProblem(LevelConfig config)
        {
            if (config == null)
            {
                return "the asset was deleted";
            }

            if (config.PropSet == null)
            {
                return "no PropSet assigned";
            }

            return $"PropSet '{config.PropSet.name}' has no baked variants";
        }
    }
}
