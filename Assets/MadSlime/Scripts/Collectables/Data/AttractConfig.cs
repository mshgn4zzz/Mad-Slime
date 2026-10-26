using UnityEngine;
namespace Collectables
{
    [CreateAssetMenu(fileName = "NewAttractConfig", menuName = "Mad Slime/Attract Config")]
    public class AttractConfig : ScriptableObject
    {
        [Tooltip("Базовая скорость притяжения предметов магнитом (юнитов/с) в точке первого контакта.")]
        [SerializeField] private float _attractionForce = 6f;

        [Tooltip("Во сколько раз разгоняется притяжение у точки захвата по сравнению с краем радиуса.")]
        [SerializeField] private float _approachMultiplier = 3f;

                [Tooltip("Крутизна кривой разгона. 1 = линейный разгон по всему радиусу, 2+ = разгон сосредоточен "
                    + "ближе к захвату.")]
        [SerializeField] private float _approachPower = 2f;

        public float AttractionForce => _attractionForce;
        public float ApproachMultiplier => _approachMultiplier;
        public float ApproachPower => _approachPower;
    }
}
