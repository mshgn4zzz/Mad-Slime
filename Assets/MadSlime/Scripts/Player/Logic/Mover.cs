using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Player.Logic
{
    public class Mover : MonoBehaviour
    {
        private Clamper _clamper;
        private Vector3 _velocity;

        public Vector3 Velocity => _velocity;

        public void Setup(Clamper clamper)
        {
            ArgumentGuard.CheckNotNull(clamper, nameof(clamper));

            _clamper = clamper;
        }

        public void Stop()
        {
            _velocity = Vector3.zero;
        }

        public void Move(Vector3 direction, float speed)
        {
            ArgumentGuard.CheckNonNegative(speed, nameof(speed));

            Vector3 startPosition = transform.position;
            Vector3 stepVelocity = Vector3.ClampMagnitude(direction, 1f) * speed;
            Vector3 desiredPosition = startPosition + (stepVelocity * Time.deltaTime);

            transform.position = _clamper.ClampPosition(desiredPosition);
            _velocity = Vector3.zero;

            if (Time.deltaTime > 0f)
            {
                _velocity = (transform.position - startPosition) / Time.deltaTime;
            }
        }
    }
}
