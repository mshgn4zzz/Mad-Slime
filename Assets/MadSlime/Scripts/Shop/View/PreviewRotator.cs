using System;
using UnityEngine;

namespace MadSlime.Shop.View
{
    public class PreviewRotator : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 45f;

        private Transform _model;
        private Vector3 _anchor;

        public event Action Rotated;

        private void Update()
        {
            if (_model == null)
            {
                return;
            }

            _model.RotateAround(_anchor, Vector3.up, _rotationSpeed * Time.unscaledDeltaTime);
            Rotated?.Invoke();
        }

        public void Setup(Transform model, Vector3 anchor)
        {
            _model = model;
            _anchor = anchor;
        }
    }
}
