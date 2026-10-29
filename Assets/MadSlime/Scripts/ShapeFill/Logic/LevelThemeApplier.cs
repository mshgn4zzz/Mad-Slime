using MadSlime.Levels.Data;
using MadSlime.Levels.Logic;
using MadSlime.Saves.Interfaces;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.ShapeFill.Logic
{
    public class LevelThemeApplier
    {
        private ILevelStorage _levelStorage;
        private LevelConfigResolver _configResolver;

        public LevelThemeApplier(ILevelStorage levelStorage, LevelConfigResolver configResolver)
        {
            ArgumentGuard.CheckNotNull(levelStorage, nameof(levelStorage));

            ArgumentGuard.CheckNotNull(configResolver, nameof(configResolver));

            _levelStorage = levelStorage;
            _configResolver = configResolver;
        }

        public void Apply(MeshRenderer floorRenderer, ShapeFillOrchestrator fillOrchestrator)
        {
            LevelConfig config = _configResolver.GetConfigFor(_levelStorage.CurrentLevel);

            ArgumentGuard.CheckAssigned(config.Theme, config.name, nameof(config.Theme));

            if (config.Theme.FloorMaterial != null)
            {
                floorRenderer.sharedMaterial = config.Theme.FloorMaterial;
            }

            Texture2D shapeTexture = PickFillShapeTexture(config.Theme);

            ArgumentGuard.CheckAssigned(shapeTexture, config.Theme.name, nameof(shapeTexture));

            fillOrchestrator.SetShapeTexture(shapeTexture);
        }

        private Texture2D PickFillShapeTexture(global::MadSlime.Levels.Data.LevelTheme theme)
        {
            if (theme.FillShapeTextures.Count == 0)
            {
                return theme.FillShapeTexture;
            }

            return theme.FillShapeTextures[Random.Range(0, theme.FillShapeTextures.Count)];
        }
    }
}
