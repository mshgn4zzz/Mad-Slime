namespace MadSlime.Roulette.Logic.StateMachine
{
    public class RouletteSessionData
    {
        public bool IsAdvertisementGranted { get; set; }
        public RouletteOutcome Outcome { get; set; }

        public void Clear()
        {
            IsAdvertisementGranted = false;
            Outcome = null;
        }
    }
}
