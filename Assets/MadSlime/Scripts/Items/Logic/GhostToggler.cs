using MadSlime.Collectables.Logic;
using MadSlime.Items.View;
using MadSlime.Player.Logic;
using UnityEngine;
using VContainer;

namespace MadSlime.Items.Logic
{
    public class GhostToggler : MonoBehaviour
    {
        private CameraObstacleDetector _detector;
        private CollectAvailability _availability;

        private void OnEnable()
        {
            _detector.Detected += OnDetected;
            _detector.Exited += OnExited;
        }

        private void OnDisable()
        {
            _detector.Detected -= OnDetected;
            _detector.Exited -= OnExited;
        }

        [Inject]
        public void Construct(CameraObstacleDetector detector, CollectAvailability availability)
        {
            _detector = detector;
            _availability = availability;
        }

        private void OnDetected(GhostAnimator animator)
        {
            if (animator == null || animator.isActiveAndEnabled == false)
            {
                return;
            }

            if (animator.TryGetComponent(out Item item))
            {
                animator.SetGhost(_availability.IsCollectable(item.Definition.Tier) == false);
                return;
            }

            animator.SetGhost(true);
        }

        private void OnExited(GhostAnimator animator)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetGhost(false);
        }
    }
}
