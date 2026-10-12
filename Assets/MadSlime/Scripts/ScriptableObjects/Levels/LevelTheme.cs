using System.Collections.Generic;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(menuName = "Mad Slime/Level Theme", fileName = "NewLevelTheme")]
    public class LevelTheme : ScriptableObject
    {
        [Tooltip("Материал пола уровня.")]
        [SerializeField] private Material _floorMaterial;

        [Tooltip("Текстура формы для сцены заливки: альфа задаёт силуэт, цвета проступают в призраке и конфетти.")]
        [SerializeField] private Texture2D _fillShapeTexture;

        [Tooltip("Случайная картинка для каждой заливки. При пустом списке используется основная текстура формы.")]
        [SerializeField] private List<Texture2D> _fillShapeTextures = new List<Texture2D>();

        public Material FloorMaterial => _floorMaterial;
        public Texture2D FillShapeTexture => _fillShapeTexture;

        public Texture2D PickFillShapeTexture()
        {
            if (_fillShapeTextures.Count == 0)
            {
                return _fillShapeTexture;
            }

            return _fillShapeTextures[Random.Range(0, _fillShapeTextures.Count)];
        }
    }
}
