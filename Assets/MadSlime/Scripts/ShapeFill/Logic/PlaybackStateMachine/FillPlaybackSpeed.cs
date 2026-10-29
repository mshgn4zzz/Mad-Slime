namespace MadSlime.ShapeFill.Logic.PlaybackStateMachine
{
    public class FillPlaybackSpeed
    {
        public FillPlaybackSpeed(float interval, float flight, float maximum)
        {
            SpawnInterval = interval;
            FlightDuration = flight;
            MaximumDuration = maximum;
        }

        public float SpawnInterval { get; }
        public float FlightDuration { get; }
        public float MaximumDuration { get; }
    }
}
