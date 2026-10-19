using CameraSystem;
using Items;
using Player;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace Collectables
{
    // Preserves scene references and the growth-driven radius for camera obstruction detection.
    [DefaultExecutionOrder(100)]
    public class CloseItemDetector : MonoBehaviour, IRadiusRecipient
    {
        private const int BufferSize = 256;

        [SerializeField, Min(0.001f)] private float _radius = 0.4f;
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private Color _gizmoColor = Color.cyan;

        private CameraFollow _cameraFollow;
        private PlayerScaler _playerScaler;
        private RaycastHit[] _buffer = new RaycastHit[BufferSize];
        private HashSet<Item> _previousTargets = new HashSet<Item>();
        private HashSet<Item> _currentTargets = new HashSet<Item>();

        public event Action<Item> Detected;

        public event Action<Item> Exited;

        public float Radius => _radius;

        [Inject]
        public void Construct(CameraFollow cameraFollow, PlayerScaler playerScaler)
        {
            _cameraFollow = cameraFollow;
            _playerScaler = playerScaler;
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
                    Item item = _buffer[i].collider.GetComponentInParent<Item>();

                    if (item != null && _currentTargets.Add(item) == true)
                    {
                        Detected?.Invoke(item);
                    }
                }
            }

            foreach (Item item in _previousTargets)
            {
                if (_currentTargets.Contains(item) == false)
                {
                    Exited?.Invoke(item);
                }
            }

            HashSet<Item> reusableTargets = _previousTargets;
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
            foreach (Item item in _previousTargets)
            {
                Exited?.Invoke(item);
            }

            _previousTargets.Clear();
            _currentTargets.Clear();
        }

        public void SetRadius(float radius)
        {
            if (radius <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            _radius = radius;
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
