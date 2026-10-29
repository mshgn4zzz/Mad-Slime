using System;
using MadSlime.Validation;

namespace MadSlime.Items.Data
{
    public class TierLookup
    {
        private TierTable _table;

        public TierLookup(TierTable table)
        {
            ArgumentGuard.CheckNotNull(table, nameof(table));

            _table = table;
        }

        public TierEntry Get(SizeTier tier)
        {
            for (int i = 0; i < _table.Entries.Count; i++)
            {
                TierEntry entry = _table.Entries[i];

                if (entry.Tier == tier)
                {
                    ArgumentGuard.CheckPositive(entry.Scale, _table.name, nameof(entry.Scale));
                    ArgumentGuard.CheckPositive(entry.Mass, _table.name, nameof(entry.Mass));

                    return entry;
                }
            }

            throw new InvalidOperationException($"{_table.name}: missing tier '{tier}'.");
        }
    }
}
