using MadSlime.CameraSystem.Data;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.CameraSystem.Logic
{
    [DefaultExecutionOrder(10)]
    public class CameraFollow : MonoBehaviour
    {
        private Transform _target;
        private CameraConfig _config;
        private Vector3 _positionVelocity;
        private Vector3 _currentOffset;
        private Vector3 _startOffset;
        private bool _isStopped;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_config, name, nameof(_config));
            ArgumentGuard.CheckAssigned(_target, name, nameof(_target));

            _startOffset = transform.position - _target.position;
            _currentOffset = _startOffset;
        }

        private void LateUpdate()
        {
            if (_isStopped || Time.deltaTime <= 0f)
            {
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                _target.position + _currentOffset,
                ref _positionVelocity,
                _config.PositionSmoothTime,
                _config.MaxPositionSpeed);
        }

        [Inject]
        public void Construct(Transform target, CameraConfig config)
        {
            _target = target;
            _config = config;
        }

        public void SetOffsetMultiplier(float offsetMultiplier)
        {
            _currentOffset = _startOffset * offsetMultiplier;
        }

        public void StopMotion()
        {
            _isStopped = true;
            _positionVelocity = Vector3.zero;
        }
    }
}
