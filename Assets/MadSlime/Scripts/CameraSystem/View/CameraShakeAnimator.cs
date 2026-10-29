using DG.Tweening;
using MadSlime.CameraSystem.Data;
using MadSlime.Collectables.Logic;
using MadSlime.Items.View;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.CameraSystem.View
{
    [DefaultExecutionOrder(30)]
    public class CameraShakeAnimator : MonoBehaviour
    {
        [SerializeField] private CameraConfig _config;

        private ItemCollector _itemCollector;
        private Vector3 _restPosition;

        private Tween _shakeTween;

        private void Awake()
        {
            _restPosition = transform.localPosition;

            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));

            ArgumentGuard.CheckAssigned(_itemCollector, name, nameof(_itemCollector));
        }

        private void OnEnable()
        {
            _itemCollector.ItemCollected += OnItemCollected;
        }

        private void OnDisable()
        {
            _itemCollector.ItemCollected -= OnItemCollected;

            StopShake();
        }

        [Inject]
        public void Construct(ItemCollector itemCollector)
        {
            _itemCollector = itemCollector;
        }

        private void OnItemCollected(Item item)
        {
            ArgumentGuard.CheckAssigned(item.Definition, item.name, nameof(item.Definition));

            if (item.Definition.Tier < _config.ShakeMinimumTier)
            {
                return;
            }

            StopShake();

            Vector2 axes = _config.ShakeAxes * _config.ShakeStrength;

            _shakeTween = transform.DOShakePosition(
                    _config.ShakeDuration,
                    new Vector3(axes.x, axes.y, 0f),
                    _config.ShakeVibrato,
                    randomness: 90f,
                    snapping: false,
                    fadeOut: true,
                    randomnessMode: _config.ShakeMode)
                .SetEase(Ease.Linear)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable);
        }

        private void StopShake()
        {
            _shakeTween?.Kill();
            _shakeTween = null;
            transform.localPosition = _restPosition;
        }
    }
}
