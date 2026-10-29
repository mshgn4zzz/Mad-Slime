using System;
using MadSlime.Collectables.Logic;
using MadSlime.Items.View;
using MadSlime.Upgrades.Logic;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Quota.Logic
{
    public class QuotaCounter
    {
        private const float QuotaFillWeight = 1f;
        private const float ForeignFillWeight = 0.1f;

        private QuotaBoard _board;
        private PlayerUpgrades _upgrades;

        private ItemCollector _attachedCollector;
        private int _collectedQuotaCount;
        private float _collectedForeignFill;
        private bool _isCompleted;

        public QuotaCounter(QuotaBoard board, PlayerUpgrades upgrades)
        {
            ArgumentGuard.CheckNotNull(board, nameof(board));

            ArgumentGuard.CheckNotNull(upgrades, nameof(upgrades));

            _board = board;
            _upgrades = upgrades;
        }

        public event Action QuotaCompleted;

        public int CollectedQuotaCount => _collectedQuotaCount;

        public int TotalQuotaTarget => _board.TotalTarget;

        public float GetFillPercent()
        {
            if (_board.TotalTarget <= 0)
            {
                return 0f;
            }

            float percent = ((_collectedQuotaCount * QuotaFillWeight) + _collectedForeignFill) / _board.TotalTarget;

            return Mathf.Clamp01(percent);
        }

        public void Attach(ItemCollector collector)
        {
            ArgumentGuard.CheckNotNull(collector, nameof(collector));

            if (_attachedCollector != null)
            {
                throw new InvalidOperationException(
                    "QuotaCounter.Attach: the counter is already attached to a collector. Detach first.");
            }

            _attachedCollector = collector;
            _attachedCollector.ItemCollected += OnItemCollected;
        }

        public void Detach()
        {
            if (_attachedCollector == null)
            {
                return;
            }

            _attachedCollector.ItemCollected -= OnItemCollected;
            _attachedCollector = null;
        }

        public void ResetSession()
        {
            _collectedQuotaCount = 0;
            _collectedForeignFill = 0f;
            _isCompleted = false;
        }

        private void CheckCompletion()
        {
            if (_isCompleted)
            {
                return;
            }

            if (_collectedQuotaCount < _board.TotalTarget)
            {
                return;
            }

            _isCompleted = true;
            QuotaCompleted?.Invoke();
        }

        private void OnItemCollected(Item item)
        {
            if (_board.TryRegisterCollected(item.Definition))
            {
                _collectedQuotaCount++;
                CheckCompletion();
                return;
            }

            _collectedForeignFill += ForeignFillWeight * _upgrades.GetForeignFillMultiplier();
        }
    }
}
