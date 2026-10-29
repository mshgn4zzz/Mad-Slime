using System.Collections.Generic;
using UnityEngine;

namespace MadSlime.Levels.Data
{
    [CreateAssetMenu(menuName = "Mad Slime/Level Theme", fileName = "NewLevelTheme")]
    public class LevelTheme : ScriptableObject
    {
        [SerializeField] private Material _floorMaterial;

        [SerializeField] private Material _backgroundMaterial;
        [SerializeField] private Material _edgeMaterial;
        [SerializeField, Min(1f)] private float _platformDepth = 12f;
        [SerializeField] private EnvironmentRoot _environmentPrefab;

        [SerializeField] private Texture2D _fillShapeTexture;

        [SerializeField] private List<Texture2D> _fillShapeTextures = new List<Texture2D>();

        public Texture2D FillShapeTexture => _fillShapeTexture;

        public IReadOnlyList<Texture2D> FillShapeTextures => _fillShapeTextures;

        public Material FloorMaterial => _floorMaterial;

        public Material BackgroundMaterial => _backgroundMaterial;

        public Material EdgeMaterial => _edgeMaterial;

        public float PlatformDepth => _platformDepth;

        public EnvironmentRoot EnvironmentPrefab => _environmentPrefab;
    }
}
