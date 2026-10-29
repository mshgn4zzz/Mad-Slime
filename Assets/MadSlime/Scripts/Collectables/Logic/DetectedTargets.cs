using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadSlime.Collectables.Logic
{
    public class DetectedTargets<T> where T : MonoBehaviour
    {
        private HashSet<T> _previous = new HashSet<T>();
        private HashSet<T> _current = new HashSet<T>();

        public event Action<T> Detected;
        public event Action<T> Exited;

        public void BeginScan()
        {
            _current.Clear();
        }

        public void Add(T target)
        {
            if (_current.Add(target))
            {
                Detected?.Invoke(target);
            }
        }

        public void CompleteScan()
        {
            foreach (T target in _previous)
            {
                if (_current.Contains(target) == false && target != null)
                {
                    Exited?.Invoke(target);
                }
            }

            HashSet<T> reusable = _previous;

            _previous = _current;
            _current = reusable;
        }

        public void Clear()
        {
            foreach (T target in _previous)
            {
                if (target != null)
                {
                    Exited?.Invoke(target);
                }
            }

            _previous.Clear();
            _current.Clear();
        }
    }
}
