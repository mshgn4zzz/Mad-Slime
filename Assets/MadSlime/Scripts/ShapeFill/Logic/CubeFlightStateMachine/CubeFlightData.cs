using UnityEngine;

namespace MadSlime.ShapeFill.Logic.CubeFlightStateMachine
{
    public class CubeFlightData
    {
        public CubeFlightData(Vector3 target, Vector3 direction, float distance, float duration)
        {
            Target = target;
            DepartureDirection = direction;
            DepartureDistance = distance;
            Duration = duration;
        }

        public Vector3 Target { get; }
        public Vector3 DepartureDirection { get; }
        public float DepartureDistance { get; }
        public float Duration { get; }
    }
}
