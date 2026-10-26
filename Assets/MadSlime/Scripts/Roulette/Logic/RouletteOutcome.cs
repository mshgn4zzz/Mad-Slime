namespace Roulette
{
    public class RouletteOutcome
    {
        public int SlotIndex { get; }
        public RoulettePrize Prize { get; }

        public RouletteOutcome(int slotIndex, RoulettePrize prize)
        {
            SlotIndex = slotIndex;
            Prize = prize;
        }
    }
}
