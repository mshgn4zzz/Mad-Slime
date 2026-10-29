using System.Collections.Generic;
using MadSlime.Levels.Data;
using UnityEngine;

namespace MadSlime.Levels.Logic
{
    public static class ZoneSpacing
    {
        public static float ResolveSpacing(SpawnZone zone, LayoutSet layout, IReadOnlyList<float> zoneRadii)
        {
            if (zone.IsAutoSpacing == false && zone.Spacing > 0f)
            {
                return zone.Spacing;
            }

            float maxRadius = 0f;

            for (int i = 0; i < zoneRadii.Count; i++)
            {
                maxRadius = Mathf.Max(maxRadius, zoneRadii[i]);
            }

            return Mathf.Max(0.5f, maxRadius * layout.AutoSpacingFactor);
        }
    }
}
