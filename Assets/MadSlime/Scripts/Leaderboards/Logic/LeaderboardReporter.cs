using Game;
using Saves;
using System;
using UnityEngine;
using VContainer;
namespace Leaderboards
{
    public class LeaderboardReporter : MonoBehaviour
    {
        [SerializeField] private YandexConfig _config;

        private ILeaderboardService _leaderboardService;
        private ICollectedItemsStorage _storage;

        [Inject]
        public void Construct(ILeaderboardService leaderboardService, ICollectedItemsStorage storage)
        {
            _leaderboardService = leaderboardService;
            _storage = storage;
        }

        private void Awake()
        {
            if (_config == null)
            {
                throw new InvalidOperationException(
                    $"{name}: YandexConfig is not assigned. Drag the YandexConfig asset into the _config field.");
            }

            if (string.IsNullOrEmpty(_config.LeaderboardName))
            {
                throw new InvalidOperationException(
                    $"{name}: LeaderboardName is empty. Fill it in the YandexConfig asset (id of the "
                        + $"leaderboard from the Yandex console).");
            }

            if (_leaderboardService == null)
            {
                throw new InvalidOperationException(
                    $"{name}: ILeaderboardService was not injected. Check that ProjectLifetimeScope "
                        + $"registers the YG2 leaderboard adapter.");
            }

            if (_storage == null)
            {
                throw new InvalidOperationException(
                    $"{name}: ICollectedItemsStorage was not injected. Check that ProjectLifetimeScope "
                        + $"registers CollectedItemsStorage as ICollectedItemsStorage and LeaderboardReporter.");
            }
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
