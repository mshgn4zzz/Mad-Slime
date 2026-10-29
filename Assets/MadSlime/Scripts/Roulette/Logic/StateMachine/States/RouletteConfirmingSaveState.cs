using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MadSlime.Saves.Interfaces;

namespace MadSlime.Roulette.Logic.StateMachine.States
{
    public class RouletteConfirmingSaveState : IRouletteState
    {
        private readonly ISaveConfirmation _confirmation;
        private CancellationTokenSource _cancellation;

        public RouletteConfirmingSaveState(ISaveConfirmation confirmation)
        {
            _confirmation = confirmation;
        }

        public event Action Confirmed;
        public event Action<Exception> Failed;

        public void Enter()
        {
            _cancellation = new CancellationTokenSource();
            ConfirmAsync(_cancellation.Token).Forget();
        }

        public void Exit()
        {
            CancellationTokenSource source = _cancellation;

            _cancellation = null;
            source?.Cancel();
            source?.Dispose();
        }

        private async UniTask ConfirmAsync(CancellationToken token)
        {
            try
            {
                await _confirmation.ConfirmSavedAsync(token);
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                if (token.IsCancellationRequested == false)
                {
                    Failed?.Invoke(exception);
                }

                return;
            }

            Confirmed?.Invoke();
        }
    }
}
