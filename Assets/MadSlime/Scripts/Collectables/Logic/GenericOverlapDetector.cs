using System;
using UnityEngine;

namespace MadSlime.Collectables.Logic
{
    public abstract class GenericOverlapDetector<T> : OverlapRadiusDetector where T : MonoBehaviour
    {
        private const int BufferSize = 256;

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private Color _gizmoColor = Color.cyan;

        private readonly Collider[] _buffer = new Collider[BufferSize];
        private DetectedTargets<T> _targets = new DetectedTargets<T>();

        public event Action<T> Detected
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

        public event Action<T> Exited
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

        protected virtual void Update()
        {
            _targets.BeginScan();

            int hitsCount = CollectHits();

            ProcessTargets(hitsCount);
            _targets.CompleteScan();
        }

        protected virtual void OnDisable()
        {
            _targets.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

        private int CollectHits()
        {
            return Physics.OverlapSphereNonAlloc(transform.position, Radius, _buffer, _layerMask);
        }

        private void ProcessTargets(int hitsCount)
        {
            for (int i = 0; i < hitsCount; i++)
            {
                if (_buffer[i].TryGetComponent(out T target) == false)
                {
                    continue;
                }

                _targets.Add(target);
            }
        }
    }
}
