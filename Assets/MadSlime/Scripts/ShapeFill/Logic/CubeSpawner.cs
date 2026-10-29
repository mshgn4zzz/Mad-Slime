using System;
using MadSlime.ShapeFill.View;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.ShapeFill.Logic
{
    public class CubeSpawner : MonoBehaviour
    {
        [SerializeField]
        private FlyingCube _cubePrefab;
        [SerializeField]
        private Transform _cubesParent;
        private int _colorId;

        private MaterialPropertyBlock _propertyBlock;

        public void Initialize()
        {
            _colorId = Shader.PropertyToID("_Color");

            ArgumentGuard.CheckAssigned(_cubePrefab, name, nameof(_cubePrefab));

            ArgumentGuard.CheckAssigned(_cubesParent, name, nameof(_cubesParent));

            if (_propertyBlock == null)
            {
                _propertyBlock = new MaterialPropertyBlock();
            }
        }

        public CubeFlightAnimator Spawn(Vector3 position, Quaternion rotation, float scale, Color color)
        {
            FlyingCube cube = Instantiate(_cubePrefab, position, rotation, _cubesParent);

            cube.transform.localScale = Vector3.one * scale;

            SetColor(cube.gameObject, color);

            if (cube.TryGetComponent(out CubeFlightAnimator flightAnimator) == false)
            {
                throw new InvalidOperationException(
                    $"{name}: the FlyingCube prefab '{_cubePrefab.name}' has no "
                    + "CubeFlightAnimator. Add one to the prefab.");
            }

            return flightAnimator;
        }

        private void SetColor(GameObject cube, Color color)
        {
            if (cube.TryGetComponent(out Renderer cubeRenderer) == false)
            {
                return;
            }

            _propertyBlock.SetColor(_colorId, color);
            cubeRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
