using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.ShapeFill.FillStateMachine.Interfaces;
using MadSlime.ShapeFill.FillStateMachine.Logic;
using MadSlime.ShapeFill.Logic;
using MadSlime.Validation;

namespace MadSlime.ShapeFill.FillStateMachine.States
{
    public class RescuingPrepareState : IFillState
    {
        private FillSessionData _fillSessionData;
        private FillSessionSaver _saver;
        private CancellationTokenSource _preparationSource;

        public RescuingPrepareState(FillSessionData fillSessionData, FillSessionSaver saver)
        {
            ArgumentGuard.CheckNotNull(fillSessionData, nameof(fillSessionData));
            ArgumentGuard.CheckNotNull(saver, nameof(saver));

            _fillSessionData = fillSessionData;
            _saver = saver;
        }

        public event Action Prepared;
        public event Action<Exception> PreparationFailed;

        public void Enter()
        {
            _preparationSource = new CancellationTokenSource();
            PrepareRescueAsync(_preparationSource.Token).Forget();
        }

        public void Exit()
        {
            _preparationSource.Cancel();
            _preparationSource.Dispose();
            _preparationSource = null;
        }

        private async UniTaskVoid PrepareRescueAsync(CancellationToken cancellationToken)
        {
            _fillSessionData.MarkRescueUsed();

            try
            {
                await _saver.CommitRescueAndSave(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                PreparationFailed?.Invoke(exception);
                return;
            }

            _fillSessionData.PrepareRescueContinuation();
            Prepared?.Invoke();
        }
    }
}
