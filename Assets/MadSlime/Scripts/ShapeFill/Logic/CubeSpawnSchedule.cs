using UnityEngine;

namespace MadSlime.ShapeFill.Logic
{
    public static class CubeSpawnSchedule
    {
        public static int GetTargetCount(float elapsed, float duration, int total)
        {
            if (total <= 0)
            {
                return 0;
            }

            if (duration <= 0f)
            {
                return total;
            }

            return Mathf.Min(total, Mathf.FloorToInt(Mathf.Clamp01(elapsed / duration) * total));
        }
    }
}
