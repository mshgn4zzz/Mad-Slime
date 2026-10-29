using UnityEngine;

namespace MadSlime.Player.Logic
{
    public class Rotator : MonoBehaviour
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;

        public void Rotate(Vector3 direction)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
