namespace MadSlime.Saves.Interfaces
{
    public interface IRouletteStorage
    {
        long LastFreeSpinUnixTime { get; }

        int SkinSpinCount { get; }

        void SetLastFreeSpinUnixTime(long unixTime);

        int CountAdSpinsSince(long windowStartUnixTime);

        void PruneAdSpinsBefore(long cutoffUnixTime);

        void RegisterAdSpin(long unixTime);

        void SetSkinSpinCount(int count);
    }
}
