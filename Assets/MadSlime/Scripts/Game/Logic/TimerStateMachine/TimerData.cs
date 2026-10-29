namespace MadSlime.Game.Logic.TimerStateMachine
{
    public class TimerData
    {
        public float Remaining { get; private set; }

        public void Reset(float duration)
        {
            Remaining = duration;
        }

        public void Consume(float delta)
        {
            Remaining -= delta;
        }

        public void Complete()
        {
            Remaining = 0f;
        }
    }
}
