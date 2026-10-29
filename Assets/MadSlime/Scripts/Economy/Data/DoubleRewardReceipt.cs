namespace MadSlime.Economy.Data
{
    public class DoubleRewardReceipt
    {
        public DoubleRewardReceipt(int baseReward)
        {
            BaseReward = baseReward;
            TotalReward = checked(baseReward * 2);
        }

        public int BaseReward { get; private set; }
        public int TotalReward { get; private set; }
    }
}
