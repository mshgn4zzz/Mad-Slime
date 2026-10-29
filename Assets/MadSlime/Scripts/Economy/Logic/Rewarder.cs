using System;
using MadSlime.Economy.Data;
using MadSlime.Saves.Interfaces;
using MadSlime.Validation;

namespace MadSlime.Economy.Logic
{
    public class Rewarder
    {
        private readonly RewardConfig _config;

        private readonly Wallet _wallet;
        private readonly ILevelStorage _levelStorage;
        private int _levelReward;
        private int _grantedAmount;
        private bool _isLevelResultCommitted;
        private bool _isWin;

        public Rewarder(RewardConfig config, Wallet wallet, ILevelStorage levelStorage)
        {
            _config = config;
            _wallet = wallet;
            _levelStorage = levelStorage;

            ArgumentGuard.CheckNotNull(_config, nameof(config));
            ArgumentGuard.CheckPositive(_config.LoseRewardDivisor, _config.name, nameof(_config.LoseRewardDivisor));
            ArgumentGuard.CheckNotNull(_wallet, nameof(wallet));
            ArgumentGuard.CheckNotNull(_levelStorage, nameof(levelStorage));
        }

        public int GrantedTotal => _grantedAmount;

        public bool IsWin => _isWin;

        public void CommitLevelResult(float percent)
        {
            if (_isLevelResultCommitted)
            {
                throw new InvalidOperationException(
                    "Rewarder: CommitLevelResult is called twice for the same level. The result is already committed.");
            }

            ArgumentGuard.CheckRange(percent, 0f, 1f, nameof(percent));

            _levelReward = GetLevelReward(_levelStorage.CurrentLevel);
            _isLevelResultCommitted = true;
            _isWin = percent >= 1f;
            _grantedAmount = CalculateReward(percent);

            if (_grantedAmount > 0)
            {
                _wallet.Add(_grantedAmount);
            }
        }

        public void CommitRescueTopUp()
        {
            if (_isLevelResultCommitted == false)
            {
                throw new InvalidOperationException(
                    "Rewarder: CommitRescueTopUp requires a committed level result. Commit the level result first.");
            }

            if (IsWin)
            {
                return;
            }

            _isWin = true;

            int topUp = _levelReward - _grantedAmount;

            if (topUp > 0)
            {
                _wallet.Add(topUp);
            }

            _grantedAmount = _levelReward;
        }

        public void ResetLevelResult()
        {
            _isLevelResultCommitted = false;
            _isWin = false;
            _grantedAmount = 0;
            _levelReward = 0;
        }

        private int GetLevelReward(int level)
        {
            ArgumentGuard.CheckPositive(level, nameof(level));

            if (_config.BaseReward < 0 || _config.GrowPower < 1f
                || float.IsNaN(_config.GrowPower) || float.IsInfinity(_config.GrowPower))
            {
                throw new InvalidOperationException($"{_config.name}: reward progression settings are invalid.");
            }

            double reward = Math.Round(
                _config.BaseReward * Math.Pow(level, _config.GrowPower), MidpointRounding.AwayFromZero);

            if (reward > int.MaxValue)
            {
                throw new OverflowException("The level reward exceeds the wallet's integer range.");
            }

            return (int)reward;
        }

        private int CalculateReward(float percent)
        {
            if (percent >= 1f)
            {
                return _levelReward;
            }

            if (percent >= _config.LoseMultiplierThreshold)
            {
                return _levelReward / _config.LoseRewardDivisor;
            }

            return 0;
        }
    }
}
