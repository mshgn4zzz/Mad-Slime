using System;
using Ads;
using Cysharp.Threading.Tasks;
using Game;

namespace ShapeFill
{
    public class FillExitFlow
    {
        private SceneStateMachine _sceneMachine;
        private AdScheduler _adScheduler;

        public FillExitFlow(SceneStateMachine sceneMachine, AdScheduler adScheduler)
        {
            if (sceneMachine == null)
            {
                throw new ArgumentNullException(nameof(sceneMachine));
            }

            if (adScheduler == null)
            {
                throw new ArgumentNullException(nameof(adScheduler));
            }

            _sceneMachine = sceneMachine;
            _adScheduler = adScheduler;
        }

        public void Exit(SceneId targetSceneId)
        {
            if (_sceneMachine.IsTransitioning || _sceneMachine.IsStopped)
            {
                return;
            }

            if (targetSceneId != SceneId.Game && targetSceneId != SceneId.Menu)
            {
                throw new ArgumentOutOfRangeException(nameof(targetSceneId), targetSceneId, "Fill can exit only to Game or Menu.");
            }

            _adScheduler.TryShowInterstitial();
            _sceneMachine.ChangeStateAsync(targetSceneId).Forget();
        }
    }
}
