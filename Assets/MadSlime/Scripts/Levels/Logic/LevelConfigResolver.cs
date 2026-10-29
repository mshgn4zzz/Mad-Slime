using System;
using MadSlime.Levels.Data;
using MadSlime.Validation;

namespace MadSlime.Levels.Logic
{
    public class LevelConfigResolver
    {
        private LevelsCatalog _catalog;

        public LevelConfigResolver(LevelsCatalog catalog)
        {
            ArgumentGuard.CheckNotNull(catalog, nameof(catalog));

            _catalog = catalog;

            foreach (LevelRange range in _catalog.Ranges)
            {
                ArgumentGuard.CheckAssigned(range, _catalog.name, nameof(_catalog.Ranges));
                ArgumentGuard.CheckPositive(range.FromLevel, _catalog.name, nameof(range.FromLevel));
                ArgumentGuard.CheckOrder(range.FromLevel, range.ToLevel, _catalog.name, nameof(_catalog.Ranges));
            }
        }

        public LevelConfig GetConfigFor(int levelNumber)
        {
            return FindRangeConfig(levelNumber);
        }

        private LevelConfig FindRangeConfig(int levelNumber)
        {
            ArgumentGuard.CheckPositive(levelNumber, nameof(levelNumber));

            int cycleLength = _catalog.Ranges[_catalog.Ranges.Count - 1].ToLevel;
            int cycleLevel = ((levelNumber - 1) % cycleLength) + 1;

            for (int i = 0; i < _catalog.Ranges.Count; i++)
            {
                if (cycleLevel >= _catalog.Ranges[i].FromLevel && cycleLevel <= _catalog.Ranges[i].ToLevel)
                {
                    return _catalog.Ranges[i].Config;
                }
            }

            throw new InvalidOperationException(
                $"LevelsCatalog '{_catalog.name}': level {levelNumber} (cycle level {cycleLevel} of {cycleLength}) "
                + $"falls into no range. Check the ranges for gaps.");
        }
    }
}
