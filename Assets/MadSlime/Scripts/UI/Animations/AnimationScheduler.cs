using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.Animations
{
    public class AnimationScheduler : MonoBehaviour
    {
        private CancellationTokenSource _sequenceSource;

        public void Play(IReadOnlyList<Action> animations, IReadOnlyList<float> delays)
        {
            if (animations.Count != delays.Count)
            {
                throw new ArgumentException("Every scheduled animation must have a delay.");
            }

            Cancel();
            _sequenceSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            RunAsync(animations, delays, _sequenceSource.Token).Forget();
        }

        public void Cancel()
        {
            if (_sequenceSource != null)
            {
                _sequenceSource.Cancel();
                _sequenceSource.Dispose();
                _sequenceSource = null;
            }
        }

        private async UniTaskVoid RunAsync(
            IReadOnlyList<Action> animations,
            IReadOnlyList<float> delays,
            CancellationToken cancellationToken)
        {
            try
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delays[i]), DelayType.Realtime, cancellationToken: cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    animations[i].Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        private void OnDisable()
        {
            Cancel();
        }
    }
}
