using CameraSystem;
using Items;
using Player;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
namespace Collectables
{
    [DefaultExecutionOrder(100)]
    public class CameraObstacleDetector : OverlapRadiusDetector
    {
        private const int BufferSize = 256;

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private Color _gizmoColor = Color.cyan;

        private CameraFollow _cameraFollow;
        private PlayerScaler _playerScaler;
        private RaycastHit[] _buffer = new RaycastHit[BufferSize];
        private HashSet<GhostAnimator> _previousTargets = new HashSet<GhostAnimator>();
        private HashSet<GhostAnimator> _currentTargets = new HashSet<GhostAnimator>();

        public event Action<GhostAnimator> Detected;

        public event Action<GhostAnimator> Exited;

        [Inject]
        public void Construct(CameraFollow cameraFollow, PlayerScaler playerScaler)
        {
            _cameraFollow = cameraFollow;
            _playerScaler = playerScaler;
        }

        private void Awake()
        {
            if (_cameraFollow == null)
            {
                throw new InvalidOperationException(
                    $"{name}: CameraFollow was not injected. Check that GameLifetimeScope "
                        + $"registers CameraFollow and CameraObstacleDetector.");
            }

            if (_playerScaler == null)
            {
                throw new InvalidOperationException(
                    $"{name}: PlayerScaler was not injected. Check that GameLifetimeScope "
                        + $"registers PlayerScaler and CameraObstacleDetector.");
            }
        }

        private void LateUpdate()
        {
            Vector3 origin = _cameraFollow.transform.position;
            Vector3 toPlayer = _playerScaler.WorldCenter - origin;
            float distance = toPlayer.magnitude;
            _currentTargets.Clear();

            if (distance > Mathf.Epsilon)
            {
                int hitsCount = CollectHits(origin, toPlayer / distance, distance);

                for (int i = 0; i < hitsCount; i++)
                {
                    GhostAnimator animator = _buffer[i].collider.GetComponentInParent<GhostAnimator>();

                    if (animator != null && animator.isActiveAndEnabled == true
                        && _currentTargets.Add(animator) == true)
                    {
                        Detected?.Invoke(animator);
                    }
                }
            }

            foreach (GhostAnimator animator in _previousTargets)
            {
                if (animator != null && _currentTargets.Contains(animator) == false)
                {
                    Exited?.Invoke(animator);
                }
            }

            HashSet<GhostAnimator> reusableTargets = _previousTargets;
            _previousTargets = _currentTargets;
            _currentTargets = reusableTargets;
        }

        private int CollectHits(Vector3 origin, Vector3 direction, float distance)
        {
            int hitsCount = Physics.SphereCastNonAlloc(origin, _radius, direction, _buffer, distance, _layerMask);

            while (hitsCount == _buffer.Length)
            {
                Array.Resize(ref _buffer, _buffer.Length * 2);
                hitsCount = Physics.SphereCastNonAlloc(origin, _radius, direction, _buffer, distance, _layerMask);
            }

            return hitsCount;
        }

        private void OnDisable()
        {
            foreach (GhostAnimator animator in _previousTargets)
            {
                if (animator != null)
                {
                    Exited?.Invoke(animator);
                }
            }

            _previousTargets.Clear();
            _currentTargets.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (_cameraFollow == null || _playerScaler == null)
            {
                return;
            }

            Vector3 origin = _cameraFollow.transform.position;
            Vector3 target = _playerScaler.WorldCenter;
            Gizmos.color = _gizmoColor;
            Gizmos.DrawLine(origin, target);
            Gizmos.DrawWireSphere(origin, _radius);
            Gizmos.DrawWireSphere(target, _radius);
        }
    }
}
