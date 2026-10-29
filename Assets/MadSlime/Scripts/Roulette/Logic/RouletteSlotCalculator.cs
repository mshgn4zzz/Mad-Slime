using System.Collections.Generic;
using MadSlime.Validation;

namespace MadSlime.Roulette.Logic
{
    public class RouletteSlotCalculator
    {
        public int CalculateIndex(IReadOnlyList<RouletteSlot> slots)
        {
            float total = 0f;

            for (int i = 0; i < slots.Count; i++)
            {
                total += slots[i].Weight;
            }

            ArgumentGuard.CheckPositive(total, nameof(RouletteSlotCalculator), nameof(total));

            float roll = UnityEngine.Random.Range(0f, total);
            int lastWeightedIndex = -1;

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Weight <= 0f)
                {
                    continue;
                }

                lastWeightedIndex = i;
                roll -= slots[i].Weight;

                if (roll < 0f)
                {
                    return i;
                }
            }

            return lastWeightedIndex;
        }
    }
}
