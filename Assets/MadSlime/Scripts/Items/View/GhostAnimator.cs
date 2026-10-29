using System;
using DG.Tweening;
using MadSlime.Items.Data;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Items.View
{
    public class GhostAnimator : MonoBehaviour
    {
        private const float SolidOpacity = 1f;

        [SerializeField] private Material _ghostMaterial;
        [SerializeField] private GhostFadeConfig _ghostFadeConfig;

        private int _opacityId;

        private Renderer[] _renderers;
        private Outline _outline;

        private Material[][] _originalMaterials;
        private Material[][] _ghostMaterials;
        private MaterialPropertyBlock _propertyBlock;
        private float _ghostTargetOpacity;
        private float _currentOpacity;
        private bool _isGhost;
        private bool _isHighlighted;

        private void Awake()
        {
            _opacityId = Shader.PropertyToID("_Opacity");

            ArgumentGuard.CheckAssigned(_ghostMaterial, name, nameof(_ghostMaterial));

            if (_ghostMaterial.HasProperty(_opacityId) == false)
            {
                throw new InvalidOperationException(
                    $"{name}: GhostMaterial has no _Opacity property. Drag a Material with the "
                    + $"MadSlime/GhostDither shader into the _ghostMaterial field.");
            }

            ArgumentGuard.CheckAssigned(_ghostFadeConfig, name, nameof(_ghostFadeConfig));

            ArgumentGuard.CheckPositive(_ghostFadeConfig.FadeDuration, name, nameof(_ghostFadeConfig.FadeDuration));

            _renderers = GetComponentsInChildren<Renderer>(true);

            ArgumentGuard.CheckNotEmpty(_renderers, name, nameof(_renderers));

            _originalMaterials = new Material[_renderers.Length][];
            _ghostMaterials = new Material[_renderers.Length][];

            for (int i = 0; i < _renderers.Length; i++)
            {
                Material[] originalSet = _renderers[i].sharedMaterials;

                _originalMaterials[i] = originalSet;
                _ghostMaterials[i] = BuildMaterialSet(_ghostMaterial, originalSet.Length);
            }

            _propertyBlock = new MaterialPropertyBlock();
            _ghostTargetOpacity = _ghostMaterial.GetFloat(_opacityId);
            _currentOpacity = SolidOpacity;
        }

        private void OnDisable()
        {
            if (_propertyBlock == null)
            {
                return;
            }

            ResetVisuals();
        }

        public void SetGhost(bool isGhost)
        {
            if (_isGhost == isGhost)
            {
                return;
            }

            _isGhost = isGhost;

            float targetOpacity;

            if (isGhost)
            {
                SwapMaterials(_ghostMaterials);
                DisableOutlines();
                targetOpacity = _ghostTargetOpacity;
            }
            else
            {
                targetOpacity = SolidOpacity;
            }

            DOTween.Kill(this);

            DOTween.To(ReadOpacity, ApplyOpacity, targetOpacity, _ghostFadeConfig.FadeDuration)
                .SetEase(_ghostFadeConfig.Ease)
                .SetTarget(this)
                .SetLink(gameObject, LinkBehaviour.KillOnDisable)
                .OnComplete(OnFadeCompleted);
        }

        public void BindOutline(Outline outline)
        {
            if (outline == null || outline.gameObject != gameObject)
            {
                throw new ArgumentException("A prepared outline on this item's root is required.", nameof(outline));
            }

            _outline = outline;
            ApplyOutline();
        }

        public void SetHighlighted(bool isHighlighted)
        {
            if (_isHighlighted == isHighlighted)
            {
                return;
            }

            _isHighlighted = isHighlighted;

            if (_isGhost)
            {
                return;
            }

            ApplyOutline();
        }

        public void ResetVisuals()
        {
            _isGhost = false;
            _isHighlighted = false;
            _currentOpacity = SolidOpacity;

            DOTween.Kill(this);
            DisableOutlines();

            RestoreOriginalMaterials();
        }

        private float ReadOpacity()
        {
            return _currentOpacity;
        }

        private void ApplyOutline()
        {
            if (_outline != null)
            {
                _outline.enabled = _isHighlighted && _isGhost == false;
            }
        }

        private void DisableOutlines()
        {
            if (_outline != null)
            {
                _outline.enabled = false;
            }
        }

        private void ApplyOpacity(float opacity)
        {
            _currentOpacity = opacity;
            _propertyBlock.SetFloat(_opacityId, opacity);

            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].SetPropertyBlock(_propertyBlock);
            }
        }

        private void SwapMaterials(Material[][] materialSets)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].sharedMaterials = materialSets[i];
            }
        }

        private Material[] BuildMaterialSet(Material material, int length)
        {
            Material[] set = new Material[length];

            for (int i = 0; i < length; i++)
            {
                set[i] = material;
            }

            return set;
        }

        private void RestoreOriginalMaterials()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    continue;
                }

                _renderers[i].SetPropertyBlock(null);
                _renderers[i].sharedMaterials = _originalMaterials[i];
            }
        }

        private void OnFadeCompleted()
        {
            if (Mathf.Approximately(_currentOpacity, SolidOpacity) == false)
            {
                return;
            }

            RestoreOriginalMaterials();
            ApplyOutline();
        }
    }
}
