using Levels;
using Saves;
using System;
using UnityEngine;
namespace ShapeFill
{
    public class LevelThemeApplier
    {
        private ILevelStorage _levelStorage;
        private LevelConfigResolver _configResolver;

        public LevelThemeApplier(ILevelStorage levelStorage, LevelConfigResolver configResolver)
        {
            if (levelStorage == null)
            {
                throw new ArgumentNullException(nameof(levelStorage));
            }

            if (configResolver == null)
            {
                throw new ArgumentNullException(nameof(configResolver));
            }

            _levelStorage = levelStorage;
            _configResolver = configResolver;
        }

        public void Apply(MeshRenderer floorRenderer, ShapeFillOrchestrator fillOrchestrator)
        {
            LevelConfig config = _configResolver.GetConfigFor(_levelStorage.CurrentLevel);

            if (config.Theme == null)
            {
                throw new InvalidOperationException(
                    $"{config.name}: LevelTheme is not assigned. Assign a theme asset in the level config.");
            }

            if (config.Theme.FloorMaterial != null)
            {
                floorRenderer.sharedMaterial = config.Theme.FloorMaterial;
            }

            Texture2D shapeTexture = config.Theme.PickFillShapeTexture();

            if (shapeTexture == null)
            {
                throw new InvalidOperationException($"{config.Theme.name}: fill shape texture is missing.");
            }

            fillOrchestrator.SetShapeTexture(shapeTexture);
        }
    }
}
