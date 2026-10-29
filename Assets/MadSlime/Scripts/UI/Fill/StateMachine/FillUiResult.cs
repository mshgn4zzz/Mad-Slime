namespace MadSlime.UI.Fill.StateMachine
{
    public class FillUiResult
    {
        public FillUiResult(bool isWin, int reward, int previous, bool isRescueAvailable)
        {
            IsWin = isWin;
            Reward = reward;
            PreviousReward = previous;
            IsRescueAvailable = isRescueAvailable;
        }

        public bool IsWin { get; }
        public int Reward { get; }
        public int PreviousReward { get; }
        public bool IsRescueAvailable { get; }
    }
}
