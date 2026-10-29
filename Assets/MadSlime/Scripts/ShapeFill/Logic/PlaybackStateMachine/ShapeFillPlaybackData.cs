namespace MadSlime.ShapeFill.Logic.PlaybackStateMachine
{
    public class ShapeFillPlaybackData
    {
        public FillResult Result { get; private set; }
        public int Spawned { get; set; }
        public int Arrived { get; set; }
        public int Target { get; set; }
        public int Capacity { get; set; }

        public void Reset(FillResult result)
        {
            Result = result;
            Spawned = 0;
            Arrived = 0;

            if (result == null)
            {
                Target = 0;
            }
            else
            {
                Target = result.TargetCubes;
            }
        }

        public int GetQuota()
        {
            if (Result == null)
            {
                return 0;
            }

            return Result.QuotaCubes;
        }
    }
}
