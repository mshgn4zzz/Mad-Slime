using UnityEngine;
namespace Collectables
{
    [CreateAssetMenu(fileName = "NewAttractConfig", menuName = "Mad Slime/Attract Config")]
    public class AttractConfig : ScriptableObject
    {
        [SerializeField] private float _pullSpeed = 6f;

        [SerializeField] private float _catchBoost = 3f;

        [SerializeField] private float _boostPower = 2f;

        public float AttractionForce => _pullSpeed;
        public float ApproachMultiplier => _catchBoost;
        public float ApproachPower => _boostPower;
    }
}
