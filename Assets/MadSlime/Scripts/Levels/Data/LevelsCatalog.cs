using System.Collections.Generic;
using UnityEngine;
namespace Levels
{
    [CreateAssetMenu(menuName = "Mad Slime/Levels Catalog", fileName = "NewLevelsCatalog")]
    public class LevelsCatalog : ScriptableObject
    {
        [Tooltip("Диапазоны уровней: с уровня X по уровень Y используется свой конфиг.")]
        [SerializeField] private List<LevelRange> _ranges = new List<LevelRange>();

        public IReadOnlyList<LevelRange> Ranges => _ranges;

        private void OnValidate()
        {
            for (int i = 0; i < _ranges.Count; i++)
            {
                LevelRange range = _ranges[i];

                if (range == null)
                {
                    Debug.LogError($"{name}: range #{i} is not assigned.");
                    continue;
                }

                if (range.FromLevel < 1 || range.ToLevel < range.FromLevel)
                {
                    Debug.LogError(
                        $"{name}: range #{i} has invalid bounds {range.FromLevel}..{range.ToLevel}. "
                                + $"FromLevel must be >= 1 and ToLevel must be >= FromLevel.");
                }
            }
        }
    }
}
