using MadSlime.Economy.Data;
using MadSlime.Saves.Interfaces;

namespace MadSlime.Economy.Logic
{
    public class DoubleRewardModel
    {
        private readonly Wallet _wallet;
        private readonly SavesTransaction _transaction;

        public DoubleRewardModel(Wallet wallet, SavesTransaction transaction)
        {
            _wallet = wallet;
            _transaction = transaction;
        }

        public DoubleRewardReceipt Grant(int baseReward)
        {
            DoubleRewardReceipt receipt = new DoubleRewardReceipt(baseReward);

            _transaction(() => _wallet.Add(baseReward));
            return receipt;
        }
    }
}
