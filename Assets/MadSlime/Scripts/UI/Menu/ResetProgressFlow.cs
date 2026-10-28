using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game;
using Saves;
using Skins;

namespace UI.Menu
{
    public class ResetProgressFlow
    {
        private SceneStateMachine _scenes;
        private ISaveConfirmation _saveConfirmation;
        private IPlayerProgressStorage _progress;
        private SkinInventory _inventory;

        public ResetProgressFlow(
            SceneStateMachine scenes,
            ISaveConfirmation saveConfirmation,
            IPlayerProgressStorage progress,
            SkinInventory inventory)
        {
            _scenes = scenes;
            _saveConfirmation = saveConfirmation;
            _progress = progress;
            _inventory = inventory;
        }

        public bool IsRunning { get; private set; }

        public async UniTask RunAsync(CancellationToken cancellationToken)
        {
            if (IsRunning || _scenes.IsTransitioning || _scenes.IsStopped)
            {
                return;
            }

            IsRunning = true;

            try
            {
                await _saveConfirmation.ConfirmSavedAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                _progress.ResetProgress();
                _inventory.Initialize();
                await _scenes.RestartCurrentAsync();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _scenes.Stop();
            }
            catch (Exception exception)
            {
                _scenes.Fail(exception);
                throw;
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}
