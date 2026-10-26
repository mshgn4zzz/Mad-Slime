using Player;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
namespace Collectables
{
    public abstract class GenericOverlapDetector<T> : OverlapRadiusDetector where T : class
    {
        private const int BufferSize = 256;

        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private Color _gizmoColor = Color.cyan;

        private Collider[] _buffer = new Collider[BufferSize];
        private HashSet<T> _previousTargets = new HashSet<T>();
        private HashSet<T> _currentTargets = new HashSet<T>();

        private static ProfilerMarker _scanMarker = new ProfilerMarker($"{typeof(T).Name}.Scan");
        private static ProfilerMarker _collectHitsMarker = new ProfilerMarker($"{typeof(T).Name}.CollectHits");
        private static ProfilerMarker _processTargetsMarker = new ProfilerMarker($"{typeof(T).Name}.ProcessTargets");
        private static ProfilerMarker _processExitsMarker = new ProfilerMarker($"{typeof(T).Name}.ProcessExits");

        public event Action<T> Detected;

        public event Action<T> Exited;

        protected virtual void Update()
        {
            using ProfilerMarker.AutoScope scanScope = _scanMarker.Auto();

            _currentTargets.Clear();
            int hitsCount = CollectHits();
            ProcessTargets(hitsCount);
            ProcessExits();
            SwapTargetSets();
        }

        private int CollectHits()
        {
            using ProfilerMarker.AutoScope collectHitsScope = _collectHitsMarker.Auto();

            int hitsCount = Physics.OverlapSphereNonAlloc(transform.position, _radius, _buffer, _layerMask);

            while (hitsCount == _buffer.Length)
            {
                Array.Resize(ref _buffer, _buffer.Length * 2);
                hitsCount = Physics.OverlapSphereNonAlloc(transform.position, _radius, _buffer, _layerMask);
            }

            return hitsCount;
        }

        private void ProcessTargets(int hitsCount)
        {
            using ProfilerMarker.AutoScope processTargetsScope = _processTargetsMarker.Auto();

            for (int i = 0; i < hitsCount; i++)
            {
                if (_buffer[i].TryGetComponent(out T target) == false || _currentTargets.Add(target) == false)
                {
                    continue;
                }

                Detected?.Invoke(target);
            }
        }

        private void ProcessExits()
        {
            using ProfilerMarker.AutoScope processExitsScope = _processExitsMarker.Auto();

            foreach (T target in _previousTargets)
            {
                if (_currentTargets.Contains(target) == false)
                {
                    if (target is UnityEngine.Object destroyed && destroyed == null)
                    {
                        continue;
                    }

                    Exited?.Invoke(target);
                }
            }
        }

        private void SwapTargetSets()
        {
            HashSet<T> reusableTargets = _previousTargets;
            _previousTargets = _currentTargets;
            _currentTargets = reusableTargets;
        }

        protected virtual void OnDisable()
        {
            foreach (T target in _previousTargets)
            {
                if (target is UnityEngine.Object destroyed && destroyed == null)
                {
                    continue;
                }

                Exited?.Invoke(target);
            }

            _previousTargets.Clear();
            _currentTargets.Clear();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
