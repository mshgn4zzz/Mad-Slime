using UnityEngine;

namespace MadSlime.Collectables.Data
{
    [CreateAssetMenu(fileName = "NewAttractConfig", menuName = "Mad Slime/Attract Config")]
    public class AttractConfig : ScriptableObject
    {
        [SerializeField] private float _pullSpeed = 6f;

        public float AttractionForce => _pullSpeed;
    }
}
