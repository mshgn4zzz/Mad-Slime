using System;
using System.Collections.Generic;
using YG;

namespace Saves
{
    public class RouletteStorage : IRouletteStorage
    {
        private SavesCore _core;

        public RouletteStorage(SavesCore core)
        {
            if (core == null)
            {
                throw new ArgumentNullException(nameof(core),
                    "RouletteStorage requires a SavesCore instance.");
            }

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
            bool removedAny = false;

            for (int i = spinTimes.Count - 1; i >= 0; i--)
            {
                if (spinTimes[i] < cutoffUnixTime)
                {
                    spinTimes.RemoveAt(i);
                    removedAny = true;
                }
            }

            if (removedAny == true)
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
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count),
                    count,
                    "RouletteStorage.SetSkinSpinCount requires a non-negative spin count.");
            }

            _core.LoadData().SkinSpinCount = count;
            _core.SaveChanges();
        }
    }
}
