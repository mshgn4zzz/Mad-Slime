using System;
using MadSlime.Skins.Logic;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Player.View
{
    public class ModelDeformer : MonoBehaviour
    {
        private const int BoundsCornerCount = 8;
        private const int CornerXMask = 1;
        private const int CornerYMask = 2;
        private const int CornerZMask = 4;
        private const float MinimumHeight = 0.0001f;
        private const float MaximumCompressionPerHeight = 0.6f;

        [SerializeField] private SkinApplier _skinApplier;
        [SerializeField] private Transform _deformationRoot;
        [SerializeField] private CapsuleCollider _playerCollider;
        [SerializeField] private Shader _shader;
        [SerializeField, Range(0f, 0.5f)] private float _pinnedFraction = 0.2f;

        private int _compressionId;
        private Renderer[] _renderers;
        private Material[][] _originalMaterials;
        private Material[][] _ownedMaterials;
        private MaterialPropertyBlock[] _blocks;
        private float _movementCompression;
        private float _collectCompression;
        private float _compression;

        private void Awake()
        {
            _compressionId = Shader.PropertyToID("_SquashCompression");

            if (_skinApplier == null || _deformationRoot == null || _playerCollider == null || _shader == null)
            {
                throw new InvalidOperationException($"{name}: skin applier, deformation root, "
                    + $"player collider and shader are required.");
            }
        }

        private void OnEnable()
        {
            _skinApplier.ModelChanged += OnModelChanged;

            if (_skinApplier.CurrentModel != null)
            {
                OnModelChanged(_skinApplier.CurrentModel);
            }
        }

        private void OnDisable()
        {
            _skinApplier.ModelChanged -= OnModelChanged;
            ReleaseMaterials();
        }

        public void SetMovementCompression(float compression)
        {
            _movementCompression = Mathf.Clamp01(compression);
            ApplyCompression();
        }

        public void SetCollectCompression(float compression)
        {
            _collectCompression = Mathf.Clamp01(compression);
            ApplyCompression();
        }

        private Bounds CalculateBounds()
        {
            ArgumentGuard.CheckNotEmpty(_renderers, name, nameof(_renderers));

            Bounds combined = new Bounds();
            bool isInitialized = false;

            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer renderer = _renderers[i];
                Bounds bounds = renderer.localBounds;
                Matrix4x4 toModel = _deformationRoot.worldToLocalMatrix * renderer.transform.localToWorldMatrix;

                for (int cornerIndex = 0; cornerIndex < BoundsCornerCount; cornerIndex++)
                {
                    Vector3 corner = bounds.min;

                    if ((cornerIndex & CornerXMask) != 0)
                    {
                        corner.x = bounds.max.x;
                    }

                    if ((cornerIndex & CornerYMask) != 0)
                    {
                        corner.y = bounds.max.y;
                    }

                    if ((cornerIndex & CornerZMask) != 0)
                    {
                        corner.z = bounds.max.z;
                    }

                    Vector3 point = toModel.MultiplyPoint3x4(corner);

                    if (isInitialized == false)
                    {
                        combined = new Bounds(point, Vector3.zero);
                        isInitialized = true;
                    }
                    else
                    {
                        combined.Encapsulate(point);
                    }
                }
            }

            return combined;
        }

        private void ApplyCompression()
        {
            float combined = 1f - ((1f - _movementCompression) * (1f - _collectCompression));
            float maximum = (1f - _pinnedFraction) * MaximumCompressionPerHeight;

            _compression = Mathf.Clamp(combined, 0f, maximum);

            if (_renderers == null)
            {
                return;
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _blocks[i].SetFloat(_compressionId, _compression);
                    _renderers[i].SetPropertyBlock(_blocks[i]);
                }
            }
        }

        private void ReleaseMaterials()
        {
            if (_renderers == null)
            {
                return;
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _originalMaterials[i] != null)
                {
                    _renderers[i].sharedMaterials = _originalMaterials[i];
                    _renderers[i].SetPropertyBlock(null);
                }

                if (_ownedMaterials[i] == null)
                {
                    continue;
                }

                for (int materialIndex = 0; materialIndex < _ownedMaterials[i].Length; materialIndex++)
                {
                    Destroy(_ownedMaterials[i][materialIndex]);
                }
            }

            _renderers = null;
            _originalMaterials = null;
            _ownedMaterials = null;
            _blocks = null;
        }

        private void OnModelChanged(GameObject model)
        {
            ReleaseMaterials();
            _renderers = model.GetComponentsInChildren<Renderer>(true);
            _originalMaterials = new Material[_renderers.Length][];
            _ownedMaterials = new Material[_renderers.Length][];
            _blocks = new MaterialPropertyBlock[_renderers.Length];

            Bounds modelBounds = CalculateBounds();
            Vector3 colliderBottom = _playerCollider.center + (Vector3.down * (_playerCollider.height * 0.5f));
            Vector3 worldBottom = _playerCollider.transform.TransformPoint(colliderBottom);
            float anchorBottom = _deformationRoot.InverseTransformPoint(worldBottom).y;
            float offset = anchorBottom - modelBounds.min.y;

            model.transform.position += _deformationRoot.TransformVector(Vector3.up * offset);

            float height = Mathf.Max(MinimumHeight, modelBounds.size.y);

            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer renderer = _renderers[i];
                Material[] originals = renderer.sharedMaterials;
                Material[] materials = new Material[originals.Length];

                _originalMaterials[i] = originals;
                _ownedMaterials[i] = materials;

                for (int materialIndex = 0; materialIndex < originals.Length; materialIndex++)
                {
                    materials[materialIndex] = new Material(originals[materialIndex]);
                    materials[materialIndex].shader = _shader;
                }

                renderer.sharedMaterials = materials;

                Matrix4x4 toModel = _deformationRoot.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
                Matrix4x4 toObject = toModel.inverse;
                MaterialPropertyBlock block = new MaterialPropertyBlock();

                renderer.GetPropertyBlock(block);
                block.SetMatrix("_SquashToModel", toModel);
                block.SetMatrix("_SquashToObject", toObject);
                block.SetMatrix("_SquashNormalToModel", toObject.transpose);
                block.SetMatrix("_SquashNormalToObject", toModel.transpose);
                block.SetFloat("_SquashBottom", anchorBottom);
                block.SetFloat("_SquashHeight", height);
                block.SetFloat("_SquashPinnedFraction", _pinnedFraction);
                _blocks[i] = block;
            }

            ApplyCompression();
        }
    }
}
