using System;
using MadSlime.Items.View;
using MadSlime.Player.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Collectables.Logic
{
    [DefaultExecutionOrder(100)]
    public class CameraObstacleDetector : OverlapRadiusDetector
    {
        private const int BufferSize = 256;

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private Color _gizmoColor = Color.cyan;

        private Camera _camera;
        private PlayerScaler _playerScaler;
        private RaycastHit[] _buffer = new RaycastHit[BufferSize];
        private DetectedTargets<GhostAnimator> _targets = new DetectedTargets<GhostAnimator>();

        public event Action<GhostAnimator> Detected
        {
            add
            {
                _targets.Detected += value;
            }
            remove
            {
                _targets.Detected -= value;
            }
        }

        public event Action<GhostAnimator> Exited
        {
            add
            {
                _targets.Exited += value;
            }
            remove
            {
                _targets.Exited -= value;
            }
        }

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_camera, name, nameof(_camera));

            ArgumentGuard.CheckAssigned(_playerScaler, name, nameof(_playerScaler));
        }

        private void LateUpdate()
        {
            Vector3 origin = _camera.transform.position;
            Vector3 toPlayer = _playerScaler.WorldCenter - origin;
            float distance = toPlayer.magnitude;

            _targets.BeginScan();

            if (distance > Mathf.Epsilon)
            {
                int hitsCount = CollectHits(origin, toPlayer / distance, distance);

                for (int i = 0; i < hitsCount; i++)
                {
                    GhostAnimator animator = _buffer[i].collider.GetComponentInParent<GhostAnimator>();

                    if (animator != null && animator.isActiveAndEnabled)
                    {
                        _targets.Add(animator);
                    }
                }
            }

            _targets.CompleteScan();
        }

        private void OnDisable()
        {
            _targets.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (_camera == null || _playerScaler == null)
            {
                return;
            }

            Vector3 origin = _camera.transform.position;
            Vector3 target = _playerScaler.WorldCenter;

            Gizmos.color = _gizmoColor;
            Gizmos.DrawLine(origin, target);
            Gizmos.DrawWireSphere(origin, Radius);
            Gizmos.DrawWireSphere(target, Radius);
        }

        [Inject]
        public void Construct(Camera camera, PlayerScaler playerScaler)
        {
            _camera = camera;
            _playerScaler = playerScaler;
        }

        private int CollectHits(Vector3 origin, Vector3 direction, float distance)
        {
            int hitsCount = Physics.SphereCastNonAlloc(
                origin,
                Radius,
                direction,
                _buffer,
                distance,
                _layerMask);

            while (hitsCount == _buffer.Length)
            {
                Array.Resize(ref _buffer, _buffer.Length * 2);
                hitsCount = Physics.SphereCastNonAlloc(
                    origin,
                    Radius,
                    direction,
                    _buffer,
                    distance,
                    _layerMask);
            }

            return hitsCount;
        }
    }
}
