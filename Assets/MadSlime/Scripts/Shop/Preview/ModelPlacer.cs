using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shop
{
    public class ModelPlacer : MonoBehaviour
    {
        private const float FaceTurnDegrees = 45f;

        [SerializeField] private PreviewRotator _rotator;
        [SerializeField] private float _padding = 0.85f;
        [SerializeField, Min(0.1f), Tooltip("Общий масштаб после нормализации размеров моделей. 1 — стандартный размер.")]
        private float _previewZoom = 1f;
        [SerializeField, Min(0.1f), Tooltip("Размер модели до применения общего масштаба: максимальная высота или диаметр при вращении.")]
        private float _normalizedSize = 2f;
        [SerializeField, Tooltip("Подъём модели над центром экрана после подгонки.")]
        private float _lift = 0f;
        [SerializeField] private Transform _modelsParent;
        [SerializeField] private Camera _camera;

        private readonly List<Vector3> _worldCorners = new List<Vector3>();

        private GameObject _currentModel;
        private Vector3 _rotationAnchor;

        private void Awake()
        {
            if (_rotator == null)
            {
                throw new InvalidOperationException($"{name}: PreviewRotator is required.");
            }

            if (_modelsParent == null)
            {
                throw new InvalidOperationException(
                    $"{name}: ModelPlacer requires _modelsParent to be assigned in the inspector.");
            }

            if (_camera == null)
            {
                throw new InvalidOperationException(
                    $"{name}: ModelPlacer requires _camera to be assigned in the inspector.");
            }
        }

        public void SetModel(GameObject model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model),
                    $"{name}: SetModel requires a non-null model.");
            }

            if (_currentModel != null)
            {
                Destroy(_currentModel);
            }

            _currentModel = Instantiate(model, _modelsParent);

            if (_currentModel.TryGetComponent(out SkinModel skinModel) == false)
            {
                throw new InvalidOperationException(
                    $"{name}: Model '{model.name}' has no SkinModel component. Add a SkinModel component to the model prefab root.");
            }

            FaceCamera(_currentModel.transform.rotation);
            FitToCamera(skinModel);
            _rotator.Setup(_currentModel.transform, _rotationAnchor);
        }

        public void Clear()
        {
            _rotator.Setup(null, Vector3.zero);

            if (_currentModel != null)
            {
                Destroy(_currentModel);
                _currentModel = null;
            }
        }

        private void FaceCamera(Quaternion baseRotation)
        {
            Vector3 toCamera = _camera.transform.position - _currentModel.transform.position;
            toCamera.y = 0f;

            if (toCamera.sqrMagnitude <= 0f)
            {
                throw new InvalidOperationException(
                    $"{name}: the preview camera is directly above the spawn point. Move the camera so the model can face it.");
            }

            float yaw = Mathf.Atan2(toCamera.x, toCamera.z) * Mathf.Rad2Deg;
            _currentModel.transform.rotation =
                Quaternion.Euler(0f, yaw + FaceTurnDegrees, 0f) * baseRotation;
        }

        private void FitToCamera(SkinModel skinModel)
        {
            Bounds worldBounds = GetWorldBounds(skinModel);
            float radius = 0f;
            foreach (Vector3 corner in _worldCorners)
            {
                Vector3 offset = corner - worldBounds.center;
                radius = Mathf.Max(radius, new Vector2(offset.x, offset.z).magnitude);
            }

            float originalSize = Mathf.Max(worldBounds.size.y, radius * 2f);
            if (originalSize <= Mathf.Epsilon)
            {
                throw new InvalidOperationException($"{name}: preview model '{skinModel.name}' has empty bounds.");
            }

            // Only the spawned preview is normalized; preserve the prefab's proportions.
            float targetSize = Mathf.Max(0.1f, _normalizedSize);
            float scale = targetSize / originalSize * Mathf.Max(0.1f, _previewZoom);
            Transform modelTransform = _currentModel.transform;
            Vector3 originalPosition = modelTransform.position;
            modelTransform.localScale *= scale;
            Vector3 scaledCenter = originalPosition + (worldBounds.center - originalPosition) * scale;
            float halfHeight = worldBounds.extents.y * scale;
            radius *= scale;
            Vector3 forward = _camera.transform.forward;
            float depthExtent = ProjectCylinder(radius, halfHeight, forward);
            float liftDepth = Vector3.Dot(Vector3.up * _lift, forward);
            // Normalization changes world size independently of the camera parent's scale.
            // Keep the entire rotating model beyond the near plane, not just its center.
            float minimumDepth = _camera.nearClipPlane + depthExtent + 0.05f - liftDepth;
            Vector3 anchor = GetViewCenter(worldBounds.center, minimumDepth) + Vector3.up * _lift;
            float farthestDepth = Vector3.Dot(anchor - _camera.transform.position, forward) + depthExtent;
            _camera.farClipPlane = Mathf.Max(_camera.farClipPlane, farthestDepth + 0.05f);
            modelTransform.position += anchor - scaledCenter;
            _rotationAnchor = anchor;

            // A vertical cylinder encloses every yaw angle, including hands and feet.
            // Project it onto the actual camera axes so a tilted camera cannot crop it.
            Vector3 right = _camera.transform.right;
            Vector3 up = _camera.transform.up;
            float horizontalExtent = ProjectCylinder(radius, halfHeight, right)
                + Mathf.Abs(Vector3.Dot(Vector3.up * _lift, right));
            float verticalExtent = ProjectCylinder(radius, halfHeight, up)
                + Mathf.Abs(Vector3.Dot(Vector3.up * _lift, up));
            float aspect = Mathf.Max(0.01f, _camera.aspect);
            float requiredSize = Mathf.Max(verticalExtent, horizontalExtent / aspect);
            float fill = Mathf.Clamp(_padding, 0.1f, 0.95f);
            float normalizedExtent = targetSize * 0.5f;
            float commonSize = Mathf.Max(
                ProjectCylinder(normalizedExtent, normalizedExtent, up),
                ProjectCylinder(normalizedExtent, normalizedExtent, right) / aspect) / fill;
            _camera.orthographicSize = Mathf.Max(commonSize, requiredSize / fill);
        }

        private static float ProjectCylinder(float radius, float halfHeight, Vector3 axis)
        {
            return radius * new Vector2(axis.x, axis.z).magnitude + halfHeight * Mathf.Abs(axis.y);
        }

        private Vector3 GetViewCenter(Vector3 position, float minimumDepth)
        {
            Vector3 forward = _camera.transform.forward;
            float depth = Mathf.Max(minimumDepth, Vector3.Dot(position - _camera.transform.position, forward));

            return _camera.transform.position + forward * depth;
        }

        private Bounds GetWorldBounds(SkinModel skinModel)
        {
            _worldCorners.Clear();
            foreach (MeshFilter meshFilter in skinModel.GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter.sharedMesh != null)
                {
                    AddWorldCorners(meshFilter.transform, meshFilter.sharedMesh.bounds);
                }
            }
            foreach (SkinnedMeshRenderer renderer in skinModel.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (renderer.sharedMesh != null)
                {
                    AddWorldCorners(renderer.transform, renderer.localBounds);
                }
            }
            if (_worldCorners.Count == 0)
            {
                throw new InvalidOperationException($"{name}: SkinModel '{skinModel.name}' has no preview mesh.");
            }
            Bounds worldBounds = new Bounds(_worldCorners[0], Vector3.zero);
            foreach (Vector3 corner in _worldCorners)
            {
                worldBounds.Encapsulate(corner);
            }
            return worldBounds;
        }

        private void AddWorldCorners(Transform sourceTransform, Bounds bounds)
        {
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                _worldCorners.Add(sourceTransform.TransformPoint(corner));
            }
        }
    }
}
