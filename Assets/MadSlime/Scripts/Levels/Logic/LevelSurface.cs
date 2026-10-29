using MadSlime.Levels.Data;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Levels.Logic
{
    public class LevelSurface
    {
        private const float BackgroundSizeMultiplier = 64f;
        private const float BackgroundThickness = 1f;

        private MeshRenderer _background;
        private MeshRenderer _edge;
        private Vector3 _meshSize;

        public LevelSurface(MeshRenderer background, MeshRenderer edge, Vector3 meshSize)
        {
            _background = background;
            _edge = edge;
            _meshSize = meshSize;
        }

        public void Apply(LevelTheme theme, Bounds floorBounds)
        {
            ArgumentGuard.CheckAssigned(theme.BackgroundMaterial, theme.name, nameof(theme.BackgroundMaterial));
            ArgumentGuard.CheckAssigned(theme.EdgeMaterial, theme.name, nameof(theme.EdgeMaterial));

            float edgeTop = floorBounds.min.y;
            Vector3 edgeCenter = floorBounds.center;

            edgeCenter.y = edgeTop - (theme.PlatformDepth * 0.5f);
            SetSurface(
                _edge,
                theme.EdgeMaterial,
                edgeCenter,
                new Vector3(
                floorBounds.size.x,
                theme.PlatformDepth,
                floorBounds.size.z));
            _edge.enabled = theme.EnvironmentPrefab == null;

            Vector3 backgroundCenter = floorBounds.center;

            backgroundCenter.y = edgeTop - theme.PlatformDepth - (BackgroundThickness * 0.5f);
            SetSurface(
                _background,
                theme.BackgroundMaterial,
                backgroundCenter,
                new Vector3(
                floorBounds.size.x * BackgroundSizeMultiplier,
                BackgroundThickness,
                floorBounds.size.z * BackgroundSizeMultiplier));
        }

        private void SetSurface(MeshRenderer renderer, Material material, Vector3 position, Vector3 size)
        {
            renderer.sharedMaterial = material;
            renderer.transform.position = position;
            renderer.transform.localScale = new Vector3(
                size.x / _meshSize.x, size.y / _meshSize.y, size.z / _meshSize.z);
        }
    }
}
