using UnityEngine;

namespace MadSlime.Geometry
{
    public static class BezierCurve
    {
        public static Vector3 GetPoint(Vector3 start, Vector3 control, Vector3 end, float progress)
        {
            float inverse = 1f - progress;

            return (inverse * inverse * start) + (2f * inverse * progress * control) + (progress * progress * end);
        }

        public static Vector3 GetPoint(
            Vector3 start,
            Vector3 departure,
            Vector3 arrival,
            Vector3 end,
            float progress)
        {
            float inverse = 1f - progress;

            return (inverse * inverse * inverse * start)
                + (3f * inverse * inverse * progress * departure)
                + (3f * inverse * progress * progress * arrival)
                + (progress * progress * progress * end);
        }
    }
}
