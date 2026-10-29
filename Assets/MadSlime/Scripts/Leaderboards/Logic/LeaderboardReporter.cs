using System;
using MadSlime.Game.Data;
using MadSlime.Leaderboards.Interfaces;
using MadSlime.Saves.Interfaces;
using MadSlime.Validation;

namespace MadSlime.Leaderboards.Logic
{
    public class LeaderboardReporter
    {
        private readonly YandexConfig _config;

        private readonly ILeaderboardService _leaderboardService;
        private readonly ICollectedItemsStorage _storage;

        public LeaderboardReporter(
            YandexConfig config,
            ILeaderboardService leaderboardService,
            ICollectedItemsStorage storage)
        {
            _config = config;
            _leaderboardService = leaderboardService;
            _storage = storage;

            ArgumentGuard.CheckNotNull(_config, nameof(config));

            if (string.IsNullOrEmpty(_config.LeaderboardName))
            {
                throw new InvalidOperationException(
                    "LeaderboardReporter: LeaderboardName is empty. Fill it in the YandexConfig asset (id of the "
                    + "leaderboard from the Yandex console).");
            }

            ArgumentGuard.CheckNotNull(_leaderboardService, nameof(leaderboardService));

            ArgumentGuard.CheckNotNull(_storage, nameof(storage));
        }

        public void Report()
        {
            if (_leaderboardService.IsAuthorized == false)
            {
                return;
            }

            _leaderboardService.SetScore(_config.LeaderboardName, _storage.CollectedItemsCount);
        }
    }
}
