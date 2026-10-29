using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Collectables.Logic;
using MadSlime.Items.View;
using MadSlime.Upgrades.Logic;
using MadSlime.Validation;
using UnityEngine;
using VContainer;

namespace MadSlime.Quota.Logic
{
    public class SmellAbilityActivator : MonoBehaviour
    {
        [SerializeField] private QuotaItemHighlighter _highlighter;

        private PlayerUpgrades _upgrades;
        private QuotaItemDetector _detector;
        private QuotaOutlineInitializer _outlineInitializer;
        private bool _isAvailable;
        private bool _isPaused = true;

        private void Awake()
        {
            ArgumentGuard.CheckAssigned(_highlighter, name, nameof(_highlighter));
            ArgumentGuard.CheckAssigned(_upgrades, name, nameof(_upgrades));
            ArgumentGuard.CheckAssigned(_detector, name, nameof(_detector));
            ArgumentGuard.CheckAssigned(_outlineInitializer, name, nameof(_outlineInitializer));
        }

        [Inject]
        public void Construct(
            PlayerUpgrades upgrades,
            QuotaItemDetector detector,
            QuotaOutlineInitializer outlineInitializer)
        {
            _upgrades = upgrades;
            _detector = detector;
            _outlineInitializer = outlineInitializer;
        }

        public async UniTask PrepareAsync(
            IReadOnlyList<Item> items,
            Action<float> progress,
            CancellationToken cancellationToken)
        {
            _isAvailable = false;
            _isPaused = true;
            ApplyState();
            cancellationToken.ThrowIfCancellationRequested();

            if (_upgrades.IsSmellUnlocked() == false)
            {
                progress?.Invoke(1f);
                return;
            }

            await _outlineInitializer.PrepareAsync(
                items,
                _highlighter.HighlightColor,
                _highlighter.OutlineWidth,
                progress,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            _isAvailable = true;
            ApplyState();
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
            ApplyState();
        }

        private void ApplyState()
        {
            bool isEnabled = _isAvailable && _isPaused == false;

            _detector.enabled = isEnabled;
            _highlighter.enabled = isEnabled;
        }
    }
}
