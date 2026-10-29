using MadSlime.Player.View;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Game.Logic
{
    public class CrawlAnimationEffect : IFinalCountdownEffect, ICountdownTimeReceiver
    {
        private CrawlAnimator _crawlAnimator;

        public CrawlAnimationEffect(CrawlAnimator crawlAnimator)
        {
            ArgumentGuard.CheckNotNull(crawlAnimator, nameof(crawlAnimator));

            _crawlAnimator = crawlAnimator;
        }

        public void Start()
        {
        }

        public void Stop()
        {
            _crawlAnimator.SetUrgency(0f);
        }

        public void UpdateProgress(float progress)
        {
            _crawlAnimator.SetUrgency(progress);
        }
    }
}
