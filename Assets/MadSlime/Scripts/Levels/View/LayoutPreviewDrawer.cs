using System.Collections.Generic;
using MadSlime.Items.Data;
using MadSlime.Levels.Data;
using MadSlime.Levels.Logic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MadSlime.Levels.View
{
    public class LayoutPreviewDrawer : MonoBehaviour
    {
        private const int PreviewRandomSeed = 0;

#if UNITY_EDITOR
        private GUIStyle _zoneLabelStyle;
#endif

        [SerializeField] private LevelGenerator _levelGenerator;
        [SerializeField] private LayoutsLibrary _library;
        [SerializeField] private TierTable _tierTable;
        [SerializeField] private PropSet _propSet;
        [UnityEngine.Serialization.FormerlySerializedAs("_mirrorX")]
        [SerializeField] private bool _isMirroredX;
        [UnityEngine.Serialization.FormerlySerializedAs("_mirrorZ")]
        [SerializeField] private bool _isMirroredZ;
        [SerializeField] private Color _boundsColor = Color.gray;
        [Space]
        [SerializeField] private LayoutSet _customLayout;

        private List<float> _zoneRadii = new List<float>(16);
        private List<Vector3> _zonePoints = new List<Vector3>(64);

        public LevelGenerator LevelGenerator => _levelGenerator;

        public LayoutsLibrary Library => _library;

        public bool IsMirroredX => _isMirroredX;

        public bool IsMirroredZ => _isMirroredZ;

        public LayoutSet CustomLayout => _customLayout;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_levelGenerator == null || _tierTable == null || _library == null)
            {
                return;
            }

            DrawMapBounds();
            DrawOrigin();

            if (_customLayout != null)
            {
                DrawLayoutZones(_customLayout, 0);
                return;
            }

            for (int layoutIndex = 0; layoutIndex < _library.Layouts.Count; layoutIndex++)
            {
                LayoutSet layout = _library.Layouts[layoutIndex];

                if (layout != null)
                {
                    DrawLayoutZones(layout, layoutIndex);
                }
            }
        }

        private float GetMaxTierScale(SpawnZone zone)
        {
            float maxScale = 1f;

            for (int i = 0; i < _tierTable.Entries.Count; i++)
            {
                TierEntry entry = _tierTable.Entries[i];

                if (entry.Tier >= zone.MinTier && entry.Tier <= zone.MaxTier)
                {
                    maxScale = Mathf.Max(maxScale, entry.Scale);
                }
            }

            return maxScale;
        }

        private void DrawLayoutZones(LayoutSet layout, int layoutIndex)
        {
            IReadOnlyList<SpawnZone> zones = layout.Zones;

            for (int i = 0; i < zones.Count; i++)
            {
                SpawnZone zone = zones[i];
                Color tierColor = GetTierColor(zone.MinTier);

                Vector2 center = zone.Center;

                if (_isMirroredX)
                {
                    center.x = -center.x;
                }

                if (_isMirroredZ)
                {
                    center.y = -center.y;
                }

                float maxScale = GetMaxTierScale(zone);

                _zoneRadii.Clear();

                for (int propIndex = 0; propIndex < _propSet.Props.Count; propIndex++)
                {
                    if (_propSet.Props[propIndex] != null)
                    {
                        _zoneRadii.Add(ItemBounds.GetRadiusXZ(_propSet.Props[propIndex]) * maxScale);
                    }
                }

                if (_zoneRadii.Count == 0)
                {
                    _zoneRadii.Add(maxScale);
                }

                float spacing = ZoneSpacing.ResolveSpacing(zone, layout, _zoneRadii);
                ZoneLayoutPlanner planner = new ZoneLayoutPlanner();
                Random.State randomState = Random.state;

                try
                {
                    Random.InitState(PreviewRandomSeed);
                    planner.CollectPlacements(zone, center, spacing, layout, _zoneRadii, zone.IsSingleType);
                }
                finally
                {
                    Random.state = randomState;
                }

                _zonePoints.Clear();

                for (int placementIndex = 0; placementIndex < planner.Placements.Count; placementIndex++)
                {
                    _zonePoints.Add(ClampToMap(planner.Placements[placementIndex].Position, spacing * 0.5f));
                }

                Vector3 worldCenter = transform.TransformPoint(new Vector3(center.x, 0f, center.y));

                switch (zone.Shape)
                {
                    case SpawnShape.Grid:
                        DrawPointsBounds(_zonePoints, tierColor);
                        break;

                    case SpawnShape.CircleGrid:
                        if (_zonePoints.Count > 0)
                        {
                            float outerRadius = GetMaxRadialDistance(_zonePoints, worldCenter);

                            DrawZoneOutline(worldCenter, outerRadius, tierColor);
                        }

                        break;

                    default:
                        DrawZoneOutline(worldCenter, zone.Radius, tierColor);
                        break;
                }

                DrawZoneDots(_zonePoints, planner.Placements, _zoneRadii, tierColor);
                DrawZoneLabel(center, zone, layoutIndex, i, planner.Placements.Count);
            }
        }

        private void DrawMapBounds()
        {
            Bounds floorBounds = _levelGenerator.GetFloorBounds();

            if (floorBounds.size.x == 0f || floorBounds.size.z == 0f)
            {
                return;
            }

            Vector3 cornerA = new Vector3(floorBounds.min.x, floorBounds.min.y, floorBounds.min.z);
            Vector3 cornerB = new Vector3(floorBounds.max.x, floorBounds.min.y, floorBounds.min.z);
            Vector3 cornerC = new Vector3(floorBounds.max.x, floorBounds.min.y, floorBounds.max.z);
            Vector3 cornerD = new Vector3(floorBounds.min.x, floorBounds.min.y, floorBounds.max.z);

            Gizmos.color = _boundsColor;
            Gizmos.DrawLine(cornerA, cornerB);
            Gizmos.DrawLine(cornerB, cornerC);
            Gizmos.DrawLine(cornerC, cornerD);
            Gizmos.DrawLine(cornerD, cornerA);
        }

        private void DrawOrigin()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }

        private float GetMaxRadialDistance(IReadOnlyList<Vector3> points, Vector3 center)
        {
            float maxDistance = 0f;

            for (int i = 0; i < points.Count; i++)
            {
                float deltaX = points[i].x - center.x;
                float deltaZ = points[i].z - center.z;

                maxDistance = Mathf.Max(maxDistance, Mathf.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ)));
            }

            return maxDistance;
        }

        private Vector3 ClampToMap(Vector3 localPosition, float margin)
        {
            Vector3 worldPosition = transform.TransformPoint(localPosition);
            Bounds floorBounds = _levelGenerator.GetFloorBounds();

            if (floorBounds.size.x == 0f || floorBounds.size.z == 0f)
            {
                return worldPosition;
            }

            worldPosition.x = Mathf.Clamp(worldPosition.x, floorBounds.min.x + margin, floorBounds.max.x - margin);
            worldPosition.z = Mathf.Clamp(worldPosition.z, floorBounds.min.z + margin, floorBounds.max.z - margin);

            return worldPosition;
        }

        private void DrawPointsBounds(IReadOnlyList<Vector3> points, Color color)
        {
            if (points.Count == 0)
            {
                return;
            }

            float minX = points[0].x;
            float maxX = points[0].x;
            float minZ = points[0].z;
            float maxZ = points[0].z;

            for (int i = 1; i < points.Count; i++)
            {
                minX = Mathf.Min(minX, points[i].x);
                maxX = Mathf.Max(maxX, points[i].x);
                minZ = Mathf.Min(minZ, points[i].z);
                maxZ = Mathf.Max(maxZ, points[i].z);
            }

            const float Padding = 0.3f;

            Vector3 cornerA = new Vector3(minX - Padding, points[0].y, minZ - Padding);
            Vector3 cornerB = new Vector3(maxX + Padding, points[0].y, minZ - Padding);
            Vector3 cornerC = new Vector3(maxX + Padding, points[0].y, maxZ + Padding);
            Vector3 cornerD = new Vector3(minX - Padding, points[0].y, maxZ + Padding);

            Gizmos.color = color;

            Gizmos.DrawLine(cornerA, cornerB);
            Gizmos.DrawLine(cornerB, cornerC);
            Gizmos.DrawLine(cornerC, cornerD);
            Gizmos.DrawLine(cornerD, cornerA);
        }

        private void DrawZoneOutline(Vector3 center, float radius, Color color)
        {
            const int segments = 36;

            Gizmos.color = color;

            Vector3 previousPoint = new Vector3(center.x + radius, center.y, center.z);

            for (int i = 1; i <= segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                float offsetX = center.x + (Mathf.Cos(angle) * radius);
                float offsetZ = center.z + (Mathf.Sin(angle) * radius);

                Vector3 nextPoint = new Vector3(offsetX, center.y, offsetZ);

                Gizmos.DrawLine(previousPoint, nextPoint);
                previousPoint = nextPoint;
            }
        }

        private void DrawZoneDots(
            IReadOnlyList<Vector3> points,
            IReadOnlyList<ZonePlacement> placements,
            IReadOnlyList<float> poolRadii,
            Color tierColor)
        {
            Gizmos.color = tierColor;

            for (int i = 0; i < points.Count; i++)
            {
                float itemRadius = poolRadii[placements[i].PoolIndex];
                float dotRadius = Mathf.Clamp(itemRadius * 0.5f, 0.05f, 0.5f);

                Gizmos.DrawSphere(points[i], dotRadius);
            }
        }

        private void DrawZoneLabel(
Vector2 center,
SpawnZone zone,
int layoutIndex,
int zoneIndex,
int positionsCount)
        {
            string tierRange = $"{zone.MinTier}-{zone.MaxTier}";
            string typeMark;

            if (zone.IsSingleType)
            {
                typeMark = " single";
            }
            else
            {
                typeMark = string.Empty;
            }

            string labelText = $"L{layoutIndex} / Zone {zoneIndex}: {zone.Shape} x{positionsCount} "
                    + $"{tierRange}{typeMark}";

            Vector3 labelPosition = transform.TransformPoint(new Vector3(center.x, 0f, center.y)) + Vector3.up;

            Handles.Label(labelPosition, labelText, GetZoneLabelStyle());
        }

        private Color GetTierColor(SizeTier tier)
        {
            switch (tier)
            {
                case SizeTier.Small:
                    return Color.green;

                case SizeTier.Medium:
                    return Color.yellow;

                case SizeTier.Large:
                    return new Color(1f, 0.5f, 0f);

                case SizeTier.Huge:
                    return new Color(1f, 0.2f, 0.2f);

                case SizeTier.Giant:
                    return new Color(0.8f, 0.2f, 1f);

                default:
                    return Color.magenta;
            }
        }

        private GUIStyle GetZoneLabelStyle()
        {
            if (_zoneLabelStyle == null)
            {
                _zoneLabelStyle = new GUIStyle();
                _zoneLabelStyle.normal.textColor = Color.yellow;
            }

            return _zoneLabelStyle;
        }

#endif
    }
}
