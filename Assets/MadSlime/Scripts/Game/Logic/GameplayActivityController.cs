using System;
using Collectables;
using Player;
using Quota;

namespace Game
{
    public class GameplayActivityController
    {
        private ItemDetector _itemDetector;
        private AttractableDetector _attractableDetector;
        private CameraObstacleDetector _obstacleDetector;
        private CrawlAnimator _crawlAnimator;
        private SmellAbilityActivator _smellAbility;
        private Movement _movement;
        private ItemCollector _itemCollector;

        public GameplayActivityController(
            ItemDetector itemDetector,
            AttractableDetector attractableDetector,
            CameraObstacleDetector obstacleDetector,
            CrawlAnimator crawlAnimator,
            SmellAbilityActivator smellAbility,
            Movement movement,
            ItemCollector itemCollector)
        {
            ArgumentGuard.CheckNotNull(itemDetector, nameof(itemDetector));

            ArgumentGuard.CheckNotNull(attractableDetector, nameof(attractableDetector));

            ArgumentGuard.CheckNotNull(obstacleDetector, nameof(obstacleDetector));

            ArgumentGuard.CheckNotNull(crawlAnimator, nameof(crawlAnimator));

            ArgumentGuard.CheckNotNull(smellAbility, nameof(smellAbility));

            ArgumentGuard.CheckNotNull(movement, nameof(movement));

            ArgumentGuard.CheckNotNull(itemCollector, nameof(itemCollector));

            _itemDetector = itemDetector;
            _attractableDetector = attractableDetector;
            _obstacleDetector = obstacleDetector;
            _crawlAnimator = crawlAnimator;
            _smellAbility = smellAbility;
            _movement = movement;
            _itemCollector = itemCollector;
        }

        public void SetRunning(bool isRunning)
        {
            _itemDetector.enabled = isRunning;
            _attractableDetector.enabled = isRunning;
            _obstacleDetector.enabled = isRunning;
            _crawlAnimator.enabled = isRunning;
            _smellAbility.SetPaused(isRunning == false);

            if (isRunning == false)
            {
                _movement.DisableControl();
                _itemCollector.PauseCollecting();
                return;
            }

            _movement.EnableControl();
            _itemCollector.StartCollecting();
        }
    }
}
