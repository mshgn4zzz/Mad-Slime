using DG.Tweening;
using UnityEngine;

namespace MadSlime.Items.Data
{
    [CreateAssetMenu(fileName = "NewGhostFadeConfig", menuName = "Mad Slime/Ghost Fade Config")]
    public class GhostFadeConfig : ScriptableObject
    {
        [SerializeField] private float _fadeDuration = 0.25f;

        [SerializeField] private Ease _ease = Ease.InOutSine;

        public float FadeDuration => _fadeDuration;

        public Ease Ease => _ease;
    }
}
