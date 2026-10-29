using System;
using MadSlime.Items.Data;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Quota.Data
{
    [Serializable]
    public class QuotaEntry
    {
        [SerializeField] private ItemDefinition _definition;
        [SerializeField] private int _targetCount = 1;

        private int _collected;

        public QuotaEntry(ItemDefinition definition, int targetCount)
        {
            ArgumentGuard.CheckNotNull(definition, nameof(definition));

            ArgumentGuard.CheckNonNegative(targetCount, nameof(targetCount));

            _definition = definition;
            _targetCount = targetCount;
        }

        public ItemDefinition Definition => _definition;

        public int TargetCount => _targetCount;

        public int Collected => _collected;

        public int GetRemaining()
        {
            return Mathf.Max(0, _targetCount - _collected);
        }

        public void RegisterCollected()
        {
            _collected++;
        }
    }
}
