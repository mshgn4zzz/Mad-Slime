using System.Collections.Generic;
using MadSlime.Saves.Interfaces;
using MadSlime.Validation;

namespace MadSlime.Saves.Logic
{
    public class RouletteStorage : IRouletteStorage
    {
        private SavesCore _core;

        public RouletteStorage(SavesCore core)
        {
            ArgumentGuard.CheckNotNull(core, nameof(core));

            _core = core;
        }

        public long LastFreeSpinUnixTime => _core.LoadData().LastFreeSpinUnixTime;

        public int SkinSpinCount => _core.LoadData().SkinSpinCount;

        public void SetLastFreeSpinUnixTime(long unixTime)
        {
            _core.LoadData().LastFreeSpinUnixTime = unixTime;
            _core.SaveChanges();
        }

        public int CountAdSpinsSince(long windowStartUnixTime)
        {
            List<long> spinTimes = _core.LoadData().RouletteAdSpinTimes;
            int count = 0;

            for (int i = 0; i < spinTimes.Count; i++)
            {
                if (spinTimes[i] >= windowStartUnixTime)
                {
                    count++;
                }
            }

            return count;
        }

        public void PruneAdSpinsBefore(long cutoffUnixTime)
        {
            List<long> spinTimes = _core.LoadData().RouletteAdSpinTimes;
            bool isAnyRemoved = false;

            for (int i = spinTimes.Count - 1; i >= 0; i--)
            {
                if (spinTimes[i] < cutoffUnixTime)
                {
                    spinTimes.RemoveAt(i);
                    isAnyRemoved = true;
                }
            }

            if (isAnyRemoved)
            {
                _core.SaveChanges();
            }
        }

        public void RegisterAdSpin(long unixTime)
        {
            _core.LoadData().RouletteAdSpinTimes.Add(unixTime);
            _core.SaveChanges();
        }

        public void SetSkinSpinCount(int count)
        {
            ArgumentGuard.CheckNonNegative(count, nameof(count));

            _core.LoadData().SkinSpinCount = count;
            _core.SaveChanges();
        }
    }
}
