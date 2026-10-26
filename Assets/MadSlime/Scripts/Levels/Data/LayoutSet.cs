using Items;
using System.Collections.Generic;
using UnityEngine;
namespace Levels
{
    [CreateAssetMenu(menuName = "Mad Slime/Layout Set", fileName = "NewLayoutSet")]
    public class LayoutSet : ScriptableObject
    {
        [Tooltip("Зоны спавна предметов уровня.")]
        [SerializeField] private List<SpawnZone> _zones = new List<SpawnZone>();

        [Tooltip("Разрешить зеркалирование раскладки при генерации — удваивает вариативность.")]
        [SerializeField] private bool _allowMirroring = true;

        [Header("Item Placement")]
                [Tooltip("Шаг авторасстояния между предметами = радиус крупнейшего предмета × этот множитель. "
                    + "Больше = предметы реже.")]
        [SerializeField] private float _autoSpacingFactor = 2.2f;

        [Tooltip("Минимальная дистанция между предметами = шаг × этот множитель. Меньше = плотнее рой.")]
        [SerializeField] private float _scatterDistanceFactor = 0.7f;

                [Tooltip("Радиус свободной зоны для Medium и более крупных предметов вокруг (0, 0). Учитывается "
                    + "весь габарит предмета.")]
        [SerializeField, Min(0f)] private float _startClearRadius = 10f;

        [Tooltip("Радиус свободной зоны для Small вокруг (0, 0). Учитывается весь габарит предмета.")]
        [SerializeField, Min(0f)] private float _smallItemStartClearRadius = 2f;

        [Tooltip("Дополнительный зазор между Large и более крупными предметами.")]
        [SerializeField, Min(0f)] private float _largeItemGap = 1f;

        public float StartClearRadius => _startClearRadius;
        public float SmallItemStartClearRadius => _smallItemStartClearRadius;
        public float LargeItemGap => _largeItemGap;
        public IReadOnlyList<SpawnZone> Zones => _zones;
        public bool AllowMirroring => _allowMirroring;
        public float AutoSpacingFactor => _autoSpacingFactor;
        public float ScatterDistanceFactor => _scatterDistanceFactor;
    }
}
