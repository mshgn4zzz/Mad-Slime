using MadSlime.Collectables.Data;
using MadSlime.Items.View;
using MadSlime.Player.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Collectables.Logic
{
    public class ItemAttractor : MonoBehaviour
    {
        [SerializeField] private AttractConfig _config;

        private CollectAvailability _collectAvailability;
        private AttractableDetector _detector;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));

            ArgumentGuard.CheckAssigned(_collectAvailability, name, nameof(_collectAvailability));

            ArgumentGuard.CheckAssigned(_detector, name, nameof(_detector));
        }

        private void OnEnable()
        {
            _detector.Detected += OnAttractableDetected;
        }

        private void OnDisable()
        {
            _detector.Detected -= OnAttractableDetected;
        }

        [Inject]
        public void Construct(
            CollectAvailability collectAvailability,
            AttractableDetector detector)
        {
            _collectAvailability = collectAvailability;
            _detector = detector;
        }

        private void OnAttractableDetected(Item attractable)
        {
            ArgumentGuard.CheckAssigned(attractable.Definition, attractable.name, nameof(attractable.Definition));

            if (_collectAvailability.IsCollectable(attractable.Definition.Tier) == false)
            {
                return;
            }

            Transform target = attractable.transform;
            Vector3 toPlayer = transform.position - target.position;

            toPlayer.y = 0f;

            target.position = Vector3.MoveTowards(
                target.position,
                target.position + toPlayer,
                _config.AttractionForce * Time.deltaTime);
        }
    }
}
