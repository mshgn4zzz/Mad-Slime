using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Validation;
using UnityEngine;

namespace MadSlime.Player.View
{
    public class GrowthAnimator : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _growDuration = 0.55f;

        [SerializeField, Min(0f)] private float _bounce = 1.7f;

        private CancellationTokenSource _growCancellationTokenSource;

        private void OnDisable()
        {
            CancelGrowth();
        }

        public void Play(float fromValue, float targetValue, Action<float> onValueChanged)
        {
            ArgumentGuard.CheckNotNull(onValueChanged, nameof(onValueChanged));

            CancelGrowth();

            _growCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                this.GetCancellationTokenOnDestroy());

            GrowAsync(fromValue, targetValue, onValueChanged, _growCancellationTokenSource.Token).Forget();
        }

        public void Stop()
        {
            CancelGrowth();
        }

        private async UniTaskVoid GrowAsync(
            float fromValue,
            float targetValue,
            Action<float> onValueChanged,
            CancellationToken cancellationToken)
        {
            float elapsedTime = 0f;

            try
            {
                while (elapsedTime < _growDuration)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    elapsedTime += Time.deltaTime;

                    float progress = Mathf.Clamp01(elapsedTime / _growDuration);

                    onValueChanged.Invoke(
                        Mathf.LerpUnclamped(fromValue, targetValue, GetBackOutProgress(progress)));

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            onValueChanged.Invoke(targetValue);
        }

        private float GetBackOutProgress(float progress)
        {
            float shiftedProgress = progress - 1f;

            return 1f
                + ((_bounce + 1f) * shiftedProgress * shiftedProgress * shiftedProgress)
                + (_bounce * shiftedProgress * shiftedProgress);
        }

        private void CancelGrowth()
        {
            if (_growCancellationTokenSource == null)
            {
                return;
            }

            _growCancellationTokenSource.Cancel();
            _growCancellationTokenSource.Dispose();
            _growCancellationTokenSource = null;
        }
    }
}
