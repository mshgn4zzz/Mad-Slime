namespace MadSlime.Roulette.Logic
{
    public class RouletteOutcome
    {
        public RouletteOutcome(int slotIndex, RoulettePrize prize)
        {
            SlotIndex = slotIndex;
            Prize = prize;
        }

        public int SlotIndex { get; }

        public RoulettePrize Prize { get; }
    }
}
