using System;
using Ads;
using Cysharp.Threading.Tasks;
using Game;

namespace ShapeFill
{
    public class FillExitFlow
    {
        private SceneNavigator _sceneNavigator;
        private AdScheduler _adScheduler;

        public FillExitFlow(SceneNavigator sceneNavigator, AdScheduler adScheduler)
        {
            if (sceneNavigator == null)
            {
                throw new ArgumentNullException(nameof(sceneNavigator));
            }

            if (adScheduler == null)
            {
                throw new ArgumentNullException(nameof(adScheduler));
            }

            _sceneNavigator = sceneNavigator;
            _adScheduler = adScheduler;
        }

        public void Exit(SceneId targetSceneId)
        {
            if (_sceneNavigator.IsTransitioning == true)
            {
                return;
            }

            _adScheduler.TryShowInterstitial();

            if (targetSceneId == SceneId.Game)
            {
                _sceneNavigator.LoadGameAsync().Forget();
                return;
            }

            _sceneNavigator.LoadMenuAsync().Forget();
        }
    }
}
