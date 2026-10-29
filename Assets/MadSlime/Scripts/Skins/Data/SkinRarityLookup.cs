using System;
using MadSlime.Validation;

namespace MadSlime.Skins.Data
{
    public class SkinRarityLookup
    {
        private SkinRarityTable _table;

        public SkinRarityLookup(SkinRarityTable table)
        {
            ArgumentGuard.CheckNotNull(table, nameof(table));

            _table = table;
        }

        public RaritySettings Get(SkinRarity rarity)
        {
            for (int i = 0; i < _table.Settings.Count; i++)
            {
                RaritySettings settings = _table.Settings[i];

                if (settings.Rarity == rarity)
                {
                    return settings;
                }
            }

            throw new InvalidOperationException($"{_table.name}: missing rarity '{rarity}'.");
        }
    }
}
