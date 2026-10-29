using System;
using System.Collections.Generic;
using MadSlime.Items.Data;
using MadSlime.Quota.Data;
using MadSlime.Validation;

namespace MadSlime.Quota.Logic
{
    public class QuotaBoard
    {
        private List<QuotaEntry> _entries = new List<QuotaEntry>();

        private int _totalTarget;

        public event Action<int, QuotaEntry> QuotaChanged;

        public event Action ResetCompleted;

        public IReadOnlyList<QuotaEntry> Entries => _entries;

        public int TotalTarget => _totalTarget;

        public void Reset(IReadOnlyList<QuotaEntry> quota)
        {
            ArgumentGuard.CheckNotNull(quota, nameof(quota));

            _entries.Clear();
            _totalTarget = 0;

            for (int i = 0; i < quota.Count; i++)
            {
                QuotaEntry entry = quota[i];

                _entries.Add(new QuotaEntry(entry.Definition, entry.TargetCount));
                _totalTarget += entry.TargetCount;
            }

            ResetCompleted?.Invoke();
        }

        public bool IsQuotaItem(ItemDefinition definition)
        {
            QuotaEntry entry = FindEntry(definition);

            if (entry == null)
            {
                return false;
            }

            return entry.Collected < entry.TargetCount;
        }

        public bool TryRegisterCollected(ItemDefinition definition)
        {
            QuotaEntry entry = FindEntry(definition);

            if (entry == null)
            {
                return false;
            }

            if (entry.Collected >= entry.TargetCount)
            {
                return false;
            }

            entry.RegisterCollected();

            QuotaChanged?.Invoke(entry.GetRemaining(), entry);

            return true;
        }

        private QuotaEntry FindEntry(ItemDefinition definition)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Definition == definition)
                {
                    return _entries[i];
                }
            }

            return null;
        }
    }
}
